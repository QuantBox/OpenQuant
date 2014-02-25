using System;
using OpenQuant.API;
using System.Collections.Generic;
using System.Reflection;

using SmartQuant.Providers;
using SmartQuant.Instruments;
using SmartQuant.FIX;
using SmartQuant.Data;

namespace QuantBox.OQ.Extensions
{
    public class ExtMarketDataFilter:MarketDataFilter
    {
        private FieldInfo NewQuoteField;
        private FieldInfo NewTradeField;
        private FieldInfo NewBarOpenField;
        private FieldInfo NewBarField;
        private FieldInfo NewMarketBarField;

        private IMarketDataProvider marketDataProvider;
        private IBarFactory factory;

        private string _NewBarOpen;
        private string _NewBar;
        private string _NewMarketBar;

        public ExtMarketDataFilter(MarketDataProvider provider)
            : this(provider,null, null, null)
        {
        }

        public ExtMarketDataFilter(MarketDataProvider provider,string Name)
        {
            switch(Name)
            {
                case "Simulator": // 3.9.2 版的混淆过后的模拟插件
                    this.Init(provider, "Jfm54PNt0q", "et95r7Su4r", "Sfk5bbMxSg");
                    break;
                default: // 反混淆后的模拟插件，CTP插件
                    this.Init(provider, "NewBarOpen", "NewBar", "NewMarketBar");
                    break;
            }
        }

        public ExtMarketDataFilter(MarketDataProvider provider, string NewBarOpen, string NewBar, string NewMarketBar)
        {
            this.Init(provider,NewBarOpen,NewBar,NewMarketBar);
        }

        private void Init(MarketDataProvider provider, string NewBarOpen, string NewBar, string NewMarketBar)
        {
            _NewBar = NewBar;
            _NewBarOpen = NewBarOpen;
            _NewMarketBar = NewMarketBar;

            //得到OpenQuant.API.MarketDataProvider内的SmartQuant.Providers.IMarketDataProvider接口
            marketDataProvider = (IMarketDataProvider)provider.GetType().GetField("provider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(provider);
            factory = marketDataProvider.BarFactory;

            // 遍历，得到对应的事件
            foreach (var e in marketDataProvider.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                switch (e.FieldType.ToString())
                {
                    case "SmartQuant.Providers.QuoteEventHandler":
                        NewQuoteField = e;
                        // 很遗憾，不能提前在保存下来
                        //(MulticastDelegate)NewQuoteField.GetValue(marketDataProvider);
                        break;
                    case "SmartQuant.Providers.TradeEventHandler":
                        NewTradeField = e;
                        break;
                    case "SmartQuant.Providers.BarEventHandler":
                        // 有三个这样的事件，怎么识别呢？
                        // 由于混淆了代码，没法识别，只能人工先判断
                        // 判断的方法，1.断点，
                        // 2.模拟下的 v8UpctWIWM(SeriesObjectEventArgs args1)
                        if (e.Name == _NewBarOpen)
                        {
                            NewBarOpenField = e;
                        }
                        else if (e.Name == _NewBar)
                        {
                            NewBarField = e;
                        }
                        else if (e.Name == _NewMarketBar)
                        {
                            NewMarketBarField = e;
                        }
                        else
                        {
                            Console.WriteLine("{0} 没有识别出来，需人工处理！", e.Name);
                        }
                        break;
                }
            }
        }


        private void EmitNewQuoteEvent(IFIXInstrument instrument, SmartQuant.Data.Quote quote)
        {
            if (quote == null)
                return;

            if (instrument == null)
                throw new ArgumentException("合约不存在,请检查是否创建了合约");

            // 本想把这行代码放在构造函数中的，结果发现有问题
            // 在QuoteMonitor中可以看到价差，但在策略中并不会触发相应的事件
            var NewQuoteDelegate = (MulticastDelegate)NewQuoteField.GetValue(marketDataProvider);

            foreach (Delegate dlg in NewQuoteDelegate.GetInvocationList())
            {
                dlg.Method.Invoke(dlg.Target, new object[] { marketDataProvider, new QuoteEventArgs(quote, instrument, marketDataProvider) });
            }

            if (factory != null)
            {
                factory.OnNewQuote(instrument, quote);
            }
        }

        private void EmitNewTradeEvent(IFIXInstrument instrument, SmartQuant.Data.Trade trade)
        {
            if (trade == null)
                return;

            if (instrument == null)
                throw new ArgumentException("合约不存在,请检查是否创建了合约");

            var NewTradeDelegate = (MulticastDelegate)NewTradeField.GetValue(marketDataProvider);

            foreach (Delegate dlg in NewTradeDelegate.GetInvocationList())
            {
                dlg.Method.Invoke(dlg.Target, new object[] { marketDataProvider, new TradeEventArgs(trade, instrument, marketDataProvider) });
            }

            if (factory != null)
            {
                factory.OnNewTrade(instrument, trade);
            }
        }

        private void EmitNewBarEvent(IFIXInstrument instrument, SmartQuant.Data.Bar bar)
        {
            if (bar == null)
                return;

            if (_NewBar == null)
                return;

            if (instrument == null)
                throw new ArgumentException("合约不存在,请检查是否创建了合约");

            if (NewBarField == null)
                throw new ArgumentException("事件没有正确识别");

            var NewBarDelegate = (MulticastDelegate)NewBarField.GetValue(marketDataProvider);

            foreach (Delegate dlg in NewBarDelegate.GetInvocationList())
            {
                dlg.Method.Invoke(dlg.Target, new object[] { marketDataProvider, new BarEventArgs(bar, instrument, marketDataProvider) });
            }
        }

        private void EmitNewBarOpenEvent(IFIXInstrument instrument, SmartQuant.Data.Bar bar)
        {
            if (bar == null)
                return;

            if (_NewBarOpen == null)
                return;

            if (instrument == null)
                throw new ArgumentException("合约不存在,请检查是否创建了合约");

            if (NewBarOpenField == null && _NewBarOpen != null)
                throw new ArgumentException("事件没有正确识别");

            var NewBarOpenDelegate = (MulticastDelegate)NewBarOpenField.GetValue(marketDataProvider);

            foreach (Delegate dlg in NewBarOpenDelegate.GetInvocationList())
            {
                dlg.Method.Invoke(dlg.Target, new object[] { marketDataProvider, new BarEventArgs(bar, instrument, marketDataProvider) });
            }
        }

        public void EmitQuote(string instrument, DateTime time, byte providerId, double bid, int bidSize, double ask, int askSize)
        {
            SmartQuant.Data.Quote quote = new SmartQuant.Data.Quote(time, bid, bidSize, ask, askSize)
            {
                ProviderId = providerId
            };

            SmartQuant.Instruments.Instrument inst = SmartQuant.Instruments.InstrumentManager.Instruments[instrument];

            EmitNewQuoteEvent(inst, quote);
        }

        public void EmitTrade(string instrument, DateTime time, byte providerId, double price, int size)
        {
            SmartQuant.Data.Trade trade = new SmartQuant.Data.Trade(time, price, size) {
                ProviderId = providerId
            };

            SmartQuant.Instruments.Instrument inst = SmartQuant.Instruments.InstrumentManager.Instruments[instrument];

            EmitNewTradeEvent(inst, trade);
        }

        public void EmitBar(string instrument,DateTime time, byte providerId, double open, double high, double low, double close, long volume,long openInt, long size)
        {
            SmartQuant.Data.Bar bar = new SmartQuant.Data.Bar(SmartQuant.Data.BarType.Time,size,time,time.AddSeconds(size), open, high, low, close, volume,openInt)
            {
                ProviderId = providerId,
                IsComplete = true,
            };

            SmartQuant.Instruments.Instrument inst = SmartQuant.Instruments.InstrumentManager.Instruments[instrument];

            EmitNewBarEvent(inst, bar);
        }

        public void EmitBarOpen(string instrument, DateTime time, byte providerId, double open, double high, double low, double close, long volume, long openInt, long size)
        {
            SmartQuant.Data.Bar bar = new SmartQuant.Data.Bar(time, open, high, low, close, volume, size)
            {
                ProviderId = providerId,
                OpenInt = openInt,
            };

            SmartQuant.Instruments.Instrument inst = SmartQuant.Instruments.InstrumentManager.Instruments[instrument];

            EmitNewBarOpenEvent(inst, bar);
        }

        public void EmitQuote(string instrument, DateTime time, double bid, int bidSize, double ask, int askSize)
        {
            EmitQuote(instrument, time,0,bid,bidSize,ask,askSize);
        }

        public void EmitTrade(string instrument, DateTime time, double price, int size)
        {
            EmitTrade(instrument, time, 0, price,size);
        }

        public void EmitBar(string instrument, DateTime time, double open, double high, double low, double close, long volume, long openInt, long size)
        {
            EmitBar(instrument, time, 0, open, high, low, close, volume, openInt, size);
        }

        public void EmitBarOpen(string instrument, DateTime time, double open, double high, double low, double close, long volume, long openInt, long size)
        {
            EmitBarOpen(instrument, time, 0, open, high, low, close, volume, openInt, size);
        }
    }
}

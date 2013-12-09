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

        private IMarketDataProvider marketDataProvider;
        private IBarFactory factory;

        public ExtMarketDataFilter(MarketDataProvider provider)
        {
            //得到OpenQuant.API.MarketDataProvider内的SmartQuant.Providers.IMarketDataProvider接口
            marketDataProvider = (IMarketDataProvider)provider.GetType().GetField("provider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(provider);
            factory = marketDataProvider.BarFactory;

            // 遍历，得到对应的两个事件
            foreach (var e in marketDataProvider.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                //Console.WriteLine(e);
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

        public void EmitQuote(string instrument, DateTime time, double bid, int bidSize, double ask, int askSize)
        {
            EmitQuote(instrument, time,0,bid,bidSize,ask,askSize);
        }

        public void EmitTrade(string instrument, DateTime time, double price, int size)
        {
            EmitTrade(instrument, time, 0, price,size);
        }
    }
}

using System;
using System.Collections.Generic;

using OpenQuant.API;
using OpenQuant.API.Engine;

using QuantBox.OQ.Extensions;

namespace QuantBox.OQ.Demo.Data
{
    public class MyExtFilter2_Scenario : Scenario
    {
        class MyExtFilter : ExtMarketDataFilter
        {
            Dictionary<string, Trade> _bar_trade = new Dictionary<string, Trade>();

            MarketDataFilter filter;

            // 构造函数不能省
            public MyExtFilter(MarketDataProvider provider, MarketDataFilter filter)
                : base(provider)
            {
                this.filter = filter;
            }

            public override Bar FilterBar(Bar bar, string symbol)
            {
                _bar_trade[symbol] = new Trade(Clock.Now, bar.Close, (int)bar.Volume);

                Trade t1, t2;

                if (_bar_trade.TryGetValue("AAPL", out t1)
                    && _bar_trade.TryGetValue("MSFT", out t2))
                {
                    EmitTrade("AAPL-MSFT", Clock.Now, t1.Price - t2.Price, 0);
                }

                // 注意，这个地方要返回
                // 不修改默认行情
                return bar;
            }

            public override Bar FilterBarOpen(Bar bar, string symbol)
            {
                _bar_trade[symbol] = new Trade(Clock.Now, bar.Close, (int)bar.Volume);

                Trade t1, t2;

                if (_bar_trade.TryGetValue("AAPL", out t1)
                    && _bar_trade.TryGetValue("MSFT", out t2))
                {
                    EmitTrade("AAPL-MSFT", Clock.Now, t1.Price - t2.Price, 0);
                }

                // 注意，这个地方要返回
                // 不修改默认行情
                return bar;
            }
        }


        public override void Run()
        {
            MarketDataProvider.Filter = new MyExtFilter(MarketDataProvider,new MyFilter());

            Start();
        }
    }


}

using System;
using System.Collections.Generic;

using OpenQuant.API;
using OpenQuant.API.Engine;

using QuantBox.OQ.Extensions;

namespace QuantBox.OQ.Demo.Data
{
    public class MyExtFilter_Scenario : Scenario
    {
        class MyExtFilter : ExtMarketDataFilter
        {
            Dictionary<string, Trade> _trades = new Dictionary<string, Trade>();

            MarketDataFilter filter;

            // 构造函数不能省
            public MyExtFilter(MarketDataProvider provider, MarketDataFilter filter)
                : base(provider)
            {
                this.filter = filter;
            }

            public override OpenQuant.API.Trade FilterTrade(OpenQuant.API.Trade trade, string symbol)
            {
                _trades[symbol] = trade;

                // 在这之前可以做自己的过滤
                Trade t = trade;
                if (filter != null)
                {
                    t = filter.FilterTrade(trade, symbol);
                }
                if (t != null)
                {
                    EmitTrade(symbol, Clock.Now, t.Price, t.Size);
                }
                else
                {
                    return null;
                }

                Trade t1, t2;

                if (_trades.TryGetValue("IF1306", out t1)
                    && _trades.TryGetValue("IF1307", out t2))
                {
                    EmitTrade("IF1306-IF1307", Clock.Now, t1.Price - t2.Price, 0);
                    EmitTrade("IF1306-IF1307*2", Clock.Now, t1.Price - t2.Price * 2.0, 0);
                }

                // 注意，这个地方一定要返回null
                // 这实际上是让插件内部的Emit不调用
                return null;
            }
        }


        public override void Run()
        {
            MarketDataProvider.Filter = new MyExtFilter(MarketDataProvider,new MyFilter());

            Start();
        }
    }


}

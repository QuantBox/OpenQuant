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
            Dictionary<string, Bar> _bars = new Dictionary<string, Bar>();

            MarketDataFilter filter;

            // 构造函数不能省
            public MyExtFilter(MarketDataProvider provider, MarketDataFilter filter)
                : base(provider, "Simulator1")
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

            // 很悲剧，EmitBar对混淆的Simulator无效，不产生事件
            public override Bar FilterBar(Bar bar, string symbol)
            {
                string key = string.Format("{0}.{1}.{2}", symbol, bar.Type, bar.Size);
                _bars[key] = bar;

                // 在这之前可以做自己的过滤
                Bar b = bar;
                if (filter != null)
                {
                    b = filter.FilterBar(bar, symbol);
                }
                if (b != null)
                {
                    EmitBar(symbol, b.DateTime, b.Open, b.High, b.Low, b.Close, b.Volume, b.OpenInt, b.Size);
                }
                else
                {
                    return null;
                }

                Bar b1, b2;
                string key1 = string.Format("{0}.{1}.{2}", "l1405", bar.Type, bar.Size);
                string key2 = string.Format("{0}.{1}.{2}", "v1405", bar.Type, bar.Size);

                if (_bars.TryGetValue(key1, out b1)
                    && _bars.TryGetValue(key2, out b2))
                {
                    // 这个地方一定要注意!!!!b1.High与b2.High由于发生的时间点不同，相减得到的High是不正确的
                    // 如果还用他来计算TR和ATR指标的话，那就更加有问题了,Low原理也是一样
                    // 如果没有用到High/Low，直接用0也不合适，图表显示会乱
                    EmitBar("l1405-v1405", bar.DateTime,
                        b1.Open - b2.Open,
                        Math.Max(b1.Open - b2.Open, b1.Close - b2.Close),
                        Math.Min(b1.Open - b2.Open, b1.Close - b2.Close),
                        b1.Close - b2.Close,
                        0, 0, bar.Size);
                }

                // 注意，这个地方一定要返回null
                // 这实际上是让插件内部的Emit不调用
                return null;
            }

            public override Bar FilterBarOpen(Bar bar, string symbol)
            {
                string key = string.Format("{0}.{1}.{2}", symbol, bar.Type, bar.Size);
                _bars[key] = bar;

                // 在这之前可以做自己的过滤
                Bar b = bar;
                if (filter != null)
                {
                    b = filter.FilterBarOpen(bar, symbol);
                }
                if (b != null)
                {
                    EmitBarOpen(symbol, b.DateTime, b.Open, b.High, b.Low, b.Close, b.Volume, b.OpenInt, b.Size);
                }
                else
                {
                    return null;
                }

                Bar b1, b2;
                string key1 = string.Format("{0}.{1}.{2}", "l1405", bar.Type, bar.Size);
                string key2 = string.Format("{0}.{1}.{2}", "v1405", bar.Type, bar.Size);

                if (_bars.TryGetValue(key1, out b1)
                    && _bars.TryGetValue(key2, out b2))
                {
                    // 这个地方一定要注意!!!!b1.High与b2.High由于发生的时间点不同，相减得到的High是不正确的
                    // 如果还用他来计算TR和ATR指标的话，那就更加有问题了,Low原理也是一样
                    // 如果没有用到High/Low，直接用0也不合适，图表显示会乱
                    EmitBarOpen("l1405-v1405", bar.DateTime,
                        b1.Open - b2.Open,
                        Math.Max(b1.Open - b2.Open, b1.Close - b2.Close),
                        Math.Min(b1.Open - b2.Open, b1.Close - b2.Close),
                        b1.Close - b2.Close,
                        0, 0, bar.Size);
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

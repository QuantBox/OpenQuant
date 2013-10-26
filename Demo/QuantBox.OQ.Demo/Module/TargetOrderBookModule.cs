using OpenQuant.API;
using QuantBox.OQ.Demo.Helper;
using System;
using System.Collections.Generic;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetOrderBookModule : Strategy
    {
        OrderBook_BothSide_Size TargetOrderBook;

        public override void OnStrategyStart()
        {
            TargetOrderBook = new OrderBook_BothSide_Size();


        }

        public override void OnBar(Bar bar)
        {
            TargetOrderBook.Sell.Set(300, 10);
            TargetOrderBook.Sell.Set(200, 5);
            TargetOrderBook.Buy.Set(100, 10);
            TargetOrderBook.Buy.Set(50, 10);
            
            // 设置目标订单列表

            // 对比订单列表

            // 撤单，防止自成交
            // 同一价位，如果全是自己挂的，可以先撤开仓单
            // 不是自己挂的，先撤后挂的
            // 按数量开仓


            // 先撤交集，
            // 挂指定价格单
            // 调整后面的价格与数量
            // 由于后面的价格与数量没有啥影响，所以基本不调,先撤单
        }

        public void Process(DualPosition dualPosition,OrderBook_BothSide_Size bothSideOrderBook)
        {
            // 防自成交
            if (bothSideOrderBook.Buy.Count > 0)
            {
                int level = bothSideOrderBook.Buy.grid.Keys[0];
                foreach (var a in dualPosition.Sell.grid)
                {
                    if (level >= a.Key)
                    {
                        dualPosition.Sell.Cancel(a.Value);
                    }
                }
            }

            
            // 上一步有计数，如果没有运行，进行下一步，看挂单数是否对上
            if (bothSideOrderBook.Buy.Count > 0)
            {
                foreach(var b in bothSideOrderBook.Buy.grid)
                {
                    int level = b.Key;
                    double size = b.Value;
                    
                    HashSet<Order> set = dualPosition.Sell.grid[level];
                    
                }
            }
            
            
        }
    }
}

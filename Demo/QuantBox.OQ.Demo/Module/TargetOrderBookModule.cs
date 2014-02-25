using OpenQuant.API;
using QuantBox.OQ.Demo.Helper;
using QuantBox.OQ.Extensions;
using System;
using System.Collections.Generic;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetOrderBookModule : Strategy
    {
        public DualPosition DualPosition;
        public TimeHelper TimeHelper;
        public PriceHelper PriceHelper;

        OrderBook_BothSide_Size TargetOrderBook;


        public override void OnStrategyStart()
        {
            TimeHelper = new TimeHelper(EnumTradingTime.COMMODITY);
            PriceHelper = new PriceHelper(Instrument.TickSize);

            TargetOrderBook = new OrderBook_BothSide_Size();

            DualPosition = new DualPosition();
            DualPosition.Sell.PriceHelper = PriceHelper;
            DualPosition.Buy.PriceHelper = PriceHelper;

            // 测试代码
            //TargetPosition = 3;
            DualPosition.Long.Qty = 0;
            DualPosition.Short.Qty = 0;
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

        // 检查对手，保证最不会自成交
        // 这次有可能把平仓单全撤了，剩下的全是开仓单，有些风险
        public int 单边防自成交(OrderBook_OneSide_Order sell,OrderBook_OneSide_Size buy)
        {
            int cnt = 0;
            // 同买卖是不可能自成交的
            if(sell.Side == buy.Side)
                return cnt;

            // 还没有挂单，不会自成交
            if (sell.Count <= 0)
                return cnt;

            // 对手单也是空的，不会自成交
            if (buy.Count <= 0)
                return cnt;

            // 取对手单的最高价
            int level = buy.Grid.Keys[0];
            // 将挂单列表中的单子撤单
            foreach(var s in sell.Grid)
            {
                if (buy.Side == OrderSide.Buy)
                {
                    if (s.Key <= level)
                    {
                        cnt += sell.Cancel(s.Value);
                    }
                }
                else
                {
                    if (s.Key >= level)
                    {
                        cnt += sell.Cancel(s.Value);
                    }
                }
            }
            return cnt;
        }

        // 这里假设已经挂的单子已经是按最优数量进行处理过，目标是直接挂单
        // 目前对前两层做智能开平，后面的全用开仓单
        // 因为后面的使用平仓单，还要处理复杂的大量开挂单的问题。
        // 实际上不能太深，因为深了占用资金
        public int 补单(OrderBook_OneSide_Order buy1, OrderBook_OneSide_Size buy2)
        {
            int cnt = 0;
            // 方向不对，不可能补单
            if (buy1.Side != buy2.Side)
                return cnt;

            int l = 0;
            // 由于在别的地方做了撤单，在这buy2中的数量是大于buy1的
            foreach(var b2 in buy2.Grid)
            {
                ++l;
                int level = b2.Key;
                double size = b2.Value;

                // 要补单的数量
                double leave = size - buy1.SizeByLevel(level);
                if (leave <= 0)
                    continue;

                double price = PriceHelper.GetPriceByLevel(level);
                cnt += (int)leave;

                // 超过两层的全用开仓
                if (l > 2)
                {
                    SendLimitOrder(buy2.Side, leave, price,
                        OpenCloseHelper.GetOpenCloseString(EnumOpenClose.OPEN));
                    continue;
                }

                // 应当对容易平仓的位置进行平仓
                // 这个地方要测试一个，我只有10持仓，先挂10手平，第二个价格再挂自动生成的单是平还是开？检查数据计算是否正确
                // 补单是一次性的，还是分两笔，还是分两笔吧
                //double canqty = DualPosition.CanCloseQty(buy2.Side);
                
                //// 检查平仓，如果补单限为一次，那就把此行注释
                //if (canqty > 0)
                //{
                //    double min = Math.Min(leave, canqty);
                //    leave -= min;
                //    SendLimitOrder(buy2.Side, min, price, 
                //        OpenCloseHelper.GetOpenCloseString(EnumOpenClose.CLOSE_TODAY));
                //}

                if (leave > 0)
                {
                    //SendLimitOrder(buy2.Side, leave, price,
                    //    OpenCloseHelper.GetOpenCloseString(DualPosition.CanClose(buy2.Side, leave)));
                }
            }

            return cnt;
        }

        // 此部分是因为确信此位置上不会挂单，所以撤了
        public int 强行撤单部分(OrderBook_OneSide_Order buy1, OrderBook_OneSide_Size buy2)
        {
            int cnt = 0;



            return cnt;
        }

        // 其它层好办，关键在第一层，第一层要保证排队和开平
        // 没有竞争者就保证平仓

        // 在第一层时，把后面几层能平的仓先平了
        // 在第二层时，把

        // 如果第一层有开仓单，把后面所有的平仓全撤了
        // 如果第一层只有自己排队，把开仓单平了


        // 排队要排第一，然后平仓单向前挪，只撤超出部分
        // 前两层，数量超过得撤，先撤排后的，如果撤了后少于指定数就不撤
        // 后两层，只要是平仓单就撤，我没法区分平仓单啊
        public int 智能撤单逻辑(OrderBook_OneSide_Order buy1, OrderBook_OneSide_Size buy2)
        {
            int cnt = 0;
            // 方向不对，不可能补单
            if (buy1.Side != buy2.Side)
                return cnt;

            // 由于在别的地方做了撤单，在这buy2中的数量是大于buy1的
            foreach (var b2 in buy2.Grid)
            {
                int level = b2.Key;
                double size = b2.Value;

                // 要补单的数量，这地方有问题，由于不对应，完全没有价格的会漏掉
                double leave = size - buy1.SizeByLevel(level);
                if (leave <= 0)
                    continue;

                double price = PriceHelper.GetPriceByLevel(level);
                cnt += (int)leave;

                // 应当对容易平仓的位置进行平仓
                // 这个地方要测试一个，我只有10持仓，先挂10手平，第二个价格再挂自动生成的单是平还是开？检查数据计算是否正确
                // 补单是一次性的，还是分两笔，还是分两笔吧
                //double canqty = DualPosition.CanCloseQty(buy2.Side);

                //// 检查平仓，如果补单限为一次，那就把此行注释
                //if (canqty > 0)
                //{
                //    double min = Math.Min(leave, canqty);
                //    leave -= min;
                //    SendLimitOrder(buy2.Side, min, price, OpenCloseHelper.GetOpenCloseString(EnumOpenClose.CLOSE_TODAY));
                //}

                //if (leave > 0)
                //{
                //    SendLimitOrder(buy2.Side, leave, price,
                //        OpenCloseHelper.GetOpenCloseString(DualPosition.CanClose(buy2.Side, leave)));
                //}
            }

            return cnt;
        }

        public override void OnOrderPartiallyFilled(Order order)
        {
            DualPosition.Filled(order);
            // 单子部分成交，不做操作，等单子完全执行完
        }


        public override void OnOrderFilled(Order order)
        {
            DualPosition.Filled(order);
            // 检查仓位是否正确,是否要发新单
        }


        public override void OnNewOrder(Order order)
        {
            DualPosition.NewOrder(order);
        }


        public override void OnOrderRejected(Order order)
        {
            DualPosition.OrderRejected(order);

            // 当前状态禁止此项操作,时间不对，应当等下次操作
        }


        public override void OnOrderCancelled(Order order)
        {
            DualPosition.OrderCancelled(order);
        }


        public override void OnOrderCancelReject(Order order)
        {
            // 撤单被拒绝，暂不操作
        }


    }
}

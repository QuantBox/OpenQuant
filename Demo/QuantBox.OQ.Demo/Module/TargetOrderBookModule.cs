using OpenQuant.API;
using QuantBox.OQ.Demo.Helper;
using QuantBox.OQ.Extensions;
using QuantBox.OQ.Extensions.OrderText;
using System;
using System.Collections.Generic;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetOrderBookModule : TargetPositionModule
    {
        // 大于N层的单子总是开仓，这样不占用平仓机会
        public double AlwaysOpenIfDepthGreatThan = 2;
        // 目标挂单
        public OrderBook_BothSide_Size TargetOrderBook;

        protected TextCommon TextParameterAsk;
        protected TextCommon TextParameterBid;

        public override void OnStrategyStart()
        {
            base.OnStrategyStart();

            // 测试用，自定义交易时间，仿真或实盘时可删除
            base.TimeHelper = new TimeHelper(new int[] { 0, 2400 }, 2100, 1458);

            TargetOrderBook = new OrderBook_BothSide_Size();
            TargetOrderBook.Sell.PriceHelper = base.PriceHelper;
            TargetOrderBook.Buy.PriceHelper = base.PriceHelper;
            TextParameterAsk = new TextCommon() { OpenClose = EnumOpenClose.OPEN };
            TextParameterBid = new TextCommon() { OpenClose = EnumOpenClose.OPEN };
        }

        public override void OnBar(Bar bar)
        {
            // 如果深度只有一层，一定要记得先清理
            TargetOrderBook.Sell.Clear();
            TargetOrderBook.Buy.Clear();

            TargetOrderBook.Sell.Set(bar.Close + 2, 7);
            TargetOrderBook.Sell.Set(bar.Close + 1, 5);
            TargetOrderBook.Buy.Set(bar.Close - 1, 2);
            TargetOrderBook.Buy.Set(bar.Close - 2, 8);


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

            base.OnBar(bar);
        }

        public override void Process()
        {
            // 非交易时段，不处理
            if (!TimeHelper.IsTradingTime())
            {
                return;
            }

            int cnt_ask = 0, cnt_bid = 0;
            cnt_ask += 单边防自成交撤单(base.DualPosition.Sell, TargetOrderBook.Buy);
            cnt_ask += 智能撤单逻辑(base.DualPosition.Sell, TargetOrderBook.Sell);

            cnt_bid += 单边防自成交撤单(base.DualPosition.Buy, TargetOrderBook.Sell);
            cnt_bid += 智能撤单逻辑(base.DualPosition.Buy, TargetOrderBook.Buy);

            if (cnt_ask == 0)
            {
                单边全面补单(base.DualPosition.Sell, TargetOrderBook.Sell);
            }

            if (cnt_bid == 0)
            {
                单边全面补单(base.DualPosition.Buy, TargetOrderBook.Buy);
            }
        }

        // 检查对手，保证不会自成交
        // 这次有可能把平仓单全撤了，剩下的全是开仓单，有些风险
        public int 单边防自成交撤单(OrderBook_OneSide_Order sell, OrderBook_OneSide_Size buy)
        {
            int cnt = 0;
            // 同买卖是不可能自成交的
            if (sell.Side == buy.Side)
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
            foreach (var s in sell.Grid)
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
        public int 单边全面补单(OrderBook_OneSide_Order buy1, OrderBook_OneSide_Size buy2)
        {
            int cnt = 0;
            // 方向不对，不可能补单
            if (buy1.Side != buy2.Side)
                return cnt;

            int l = 0;
            // 由于在别的地方做了撤单，在这buy2中的数量是大于等于buy1的
            foreach (var b2 in buy2.Grid)
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

                TextCommon tp = buy1.Side == OrderSide.Buy ? TextParameterBid : TextParameterAsk;
                PositionRecord LongShort = buy1.Side == OrderSide.Buy ? base.DualPosition.Short : base.DualPosition.Long;

                // 超过两层的全用开仓
                if (l > AlwaysOpenIfDepthGreatThan)
                {
                    tp.OpenClose = EnumOpenClose.OPEN;
                    tp.Text = string.Format("{0}层，开仓补单", l);
                }
                else
                {
                    // 计算开平
                    double q = CloseTodayHelper.GetCloseAndQty(LongShort, out tp.OpenClose);
                    if (q < leave)
                    {
                        tp.OpenClose = EnumOpenClose.OPEN;
                        tp.Text = string.Format("可平量{0}不够，全开仓{1}", q, leave);
                    }
                    else
                    {
                        tp.Text = string.Format("可平量{0}，全开仓{1}", q, leave);
                    }
                }

                // 入场下单
                SendLimitOrder(buy2.Side, leave, price, tp.ToString());
            }

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

            // 撤单时按已有的挂单进行处理
            foreach (var b1 in buy1.Grid)
            {
                int level = b1.Key;

                double size2 = buy2.SizeByLevel(level);
                if (size2 <= 0)
                {
                    // 发现目标价上量为0，全撤
                    cnt += buy1.Cancel(b1.Value);
                    continue;
                }

                double size1 = buy1.Size(b1.Value);
                if (size1 <= size2)
                {
                    // 量少于目标量，不动，等着补单
                    continue;
                }

                // 要撤单的量，实际上还剩的挂单量不由人为控制，所以这个地方可以多撤
                double leave = size1 - size2;
                // 现在这个价位上挂了很多单，是撤开仓，还是撤新挂的？
                double count = 0;
                foreach (Order o in b1.Value.Reverse())
                {
                    count += o.LeavesQty;
                    if (count >= leave)
                        break;

                    o.Cancel();
                    ++cnt;
                }
            }

            return cnt;
        }
    }
}
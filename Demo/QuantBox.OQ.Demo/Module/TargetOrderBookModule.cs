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

        // 特殊用途，如果是因为为了自动调整而导致的撤单，不改变挂单表
        private HashSet<Order> cancelList = new HashSet<Order>();

        public override void OnStrategyStart()
        {
            base.OnStrategyStart();

            TargetOrderBook = new OrderBook_BothSide_Size();
            TargetOrderBook.Sell.PriceHelper = base.PriceHelper;
            TargetOrderBook.Buy.PriceHelper = base.PriceHelper;
            TextParameterAsk = new TextCommon() { OpenClose = EnumOpenClose.OPEN };
            TextParameterBid = new TextCommon() { OpenClose = EnumOpenClose.OPEN };
        }

        private void SubOrder(Order order)
        {
            // 市价不会挂单，所以不会记录到目标挂单助手中
            if (order.Type != OrderType.Limit)
                return;

            double price = order.Price;
            double size = order.LastQty;

            if(order.Side == OrderSide.Buy)
            {
                TargetOrderBook.Buy.Sub(price,size);
            }
            else
            {
                TargetOrderBook.Sell.Sub(price, size);
            }
        }

        // 这下面有问题，不能这么直接减，因为撤单可能只是中间状态，后面还要补上来的
        // 而其它状态可以认为单子改变，可以修改了
        // 但如果是手动撤单，因为目标挂单没减，会再自动重挂
        // 但如果这么一减，想挂单的数就少了，系统的撤单重挂功能就无效了
        public override void OnOrderCancelled(Order order)
        {
            lock(this)
            {
                // 如果不是通过双向持仓工具，而是手工下单，就不会记录下来
                // 这样还是有问题，如果是使用的双向持仓工具，手工下单就会出错

                // 有这个单子，是调整所产生，不改变挂单表
                if (!cancelList.Remove(order))
                {
                    // 没有找到，表示非调整，要改变挂单表
                    SubOrder(order);
                }
                    
                base.OnOrderCancelled(order);
            }
        }

        public override void OnOrderRejected(Order order)
        {
            lock(this)
            {
                SubOrder(order);
                base.OnOrderRejected(order);
            }
        }

        public override void OnOrderPartiallyFilled(Order order)
        {
            lock (this)
            {
                SubOrder(order);
                base.OnOrderPartiallyFilled(order);
            }
        }

        public override void OnOrderFilled(Order order)
        {
            lock (this)
            {
                SubOrder(order);
                base.OnOrderFilled(order);
            }
        }

        public override void Process()
        {
            lock(this)
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
        }

        // 检查对手，保证不会自成交
        // 这次有可能把平仓单全撤了，剩下的全是开仓单，有些风险
        public int 单边防自成交撤单(OrderBook_OneSide_Order sell, OrderBook_OneSide_Size buy)
        {
            lock(this)
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
                            cancelList.UnionWith(s.Value);
                            cnt += sell.Cancel(s.Value);
                        }
                    }
                    else
                    {
                        if (s.Key >= level)
                        {
                            cancelList.UnionWith(s.Value);
                            cnt += sell.Cancel(s.Value);
                        }
                    }
                }
                return cnt;
            }
        }

        // 这里假设已经挂的单子已经是按最优数量进行处理过，目标是直接挂单
        // 目前对前两层做智能开平，后面的全用开仓单
        // 因为后面的使用平仓单，还要处理复杂的大量开挂单的问题。
        // 实际上不能太深，因为深了占用资金
        public int 单边全面补单(OrderBook_OneSide_Order buy1, OrderBook_OneSide_Size buy2)
        {
            lock(this)
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
            lock(this)
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
                        cancelList.UnionWith(b1.Value);
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

                        cancelList.Add(o);
                        o.Cancel();
                        ++cnt;
                    }
                }

                return cnt;
            }
        }
    }
}
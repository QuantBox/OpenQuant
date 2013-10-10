using OpenQuant.API;
using QuantBox.CSharp2CTP;
using QuantBox.Helper.CTP;
using QuantBox.OQ.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetPositionModule : Strategy
    {
        public int[] WorkingTime_Financial = { 915, 1130, 1300, 1515 }; //IF
        public int[] WorkingTime_Commodity = { 900, 1015, 1030, 1130, 1330, 1500 }; //商品
        public int[] WorkingTime_AuAg = { 0, 230, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//au,ag

        public DualPosition dualPosition;
        /// <summary>
        /// 目标仓位
        /// </summary>
        public double TargetPosition = 0;
        /// <summary>
        /// 每笔最大手数
        /// </summary>
        public double MaxQtyPerLot = 5;
        /// <summary>
        /// 小于等于此数量的开仓单自动以市价发送
        /// </summary>
        public double MarketOpenPriceThreshold = 5;
        /// <summary>
        /// 小于等于此数量的平仓单自动以市价发送
        /// </summary>
        public double MarketClosePriceThreshold = 20;
        /// <summary>
        /// 交易时段
        /// </summary>
        public int[] WorkingTime;

        public override void OnStrategyStart()
        {
            WorkingTime = WorkingTime_Commodity;
            dualPosition = new DualPosition();

            // 测试代码
            TargetPosition = 3;
            dualPosition.Long.Qty = 0;
            dualPosition.Short.Qty = 0;
        }

        public override void OnTrade(Trade trade)
        {
            Process();
        }

        public override void OnQuote(Quote quote)
        {
            Process();
        }

        public override void OnBar(Bar bar)
        {
            Process();
        }

        private bool IsWorkingTime()
        {
            // 交易时间处理，时间不对就不发单
            int time = Clock.Now.Hour * 100 + Clock.Now.Minute;
            int index = -1;
            for (int i = 0; i < WorkingTime.Length; ++i)
            {
                if (time >= WorkingTime[i])
                {
                    index = i;
                    break;
                }
            }

            if (index % 2 == 0)
            {
                // 休息时间
                return true;
            }

            return false;
        }

        // 最小手续费处理原则
        public void Process()
        {
            // 计算仓差
            double dif = TargetPosition - dualPosition.NetQty;
            double qty = 0;
            OrderSide Side = OrderSide.Buy;
            EnumOpenClose oc = EnumOpenClose.OPEN;

            if (!IsWorkingTime())
            {
                return;
            }

            if (dif == 0)// 持仓量相等
            {
                // 把所有的挂单先撤了
            }
            else if (dif > 0 && !dualPosition.IsPending)// 表示要增加净持仓
            {
                // 是否有在途增仓订单,超数了
                // 是否有在途减仓订单,全取消息
                qty = dif;
                Side = OrderSide.Buy;

                if (dualPosition.Short.Qty > 0)
                {
                    // 按最小数量进行平仓
                    qty = Math.Min(qty, dualPosition.Short.Qty);
                    oc = EnumOpenClose.CLOSE;
                }
            }
            else if (!dualPosition.IsPending) // 减少净持仓
            {
                qty = -dif;
                Side = OrderSide.Sell;

                if (dualPosition.Long.Qty > 0)
                {
                    // 按最小数量进行平仓
                    qty = Math.Min(qty, dualPosition.Long.Qty);
                    oc = EnumOpenClose.CLOSE;
                }
            }

            if (qty > 0)
            {
                qty = Math.Min(qty, MaxQtyPerLot);

                // 下单
                SendOrder(Side, oc, qty);
            }
        }

        // 得到当前对手价
        private double GetPrice(OrderSide side)
        {
            Quote quote = Quote;
            Trade trade = Trade;
            Bar bar = Bar;

            if (quote != null)
            {
                if (side == OrderSide.Buy)
                {
                    if (quote.Ask != 0)
                        return quote.Ask;
                }
                else
                {
                    if (quote.Bid != 0)
                        return quote.Bid;
                }
            }

            if (trade != null)
                if (trade.Price != 0)
                    return trade.Price;

            if (bar != null)
            {
                if (bar.Close != 0)
                    return bar.Close;

                if (bar.Open != 0)
                    return bar.Open;
            }

            return 0;
        }

        // 在对手价上加一定跳数
        private double GetPrice(OrderSide side, double jump)
        {
            double price = GetPrice(side);
            if (side == OrderSide.Buy)
            {
                price += jump * Instrument.TickSize;
            }
            else
            {
                price -= jump * Instrument.TickSize;
            }
            return price;
        }

        // 下单操作
        private void SendOrder(OrderSide side, EnumOpenClose oc, double qty)
        {
            if (qty <= 0)
            {
                return;
            }

            // 为减少滑点，对数量少的单子直接市价单
            bool bMarketOrder = false;
            if (EnumOpenClose.OPEN == oc)
            {
                if (qty <= MarketOpenPriceThreshold)
                    bMarketOrder = true;
            }
            else
            {
                if (qty <= MarketClosePriceThreshold)
                    bMarketOrder = true;
            }

            if (bMarketOrder)
            {
                SendMarketOrder(side, qty, OpenCloseHelper.GetOpenCloseString(oc));
            }
            else
            {
                SendLimitOrder(side, qty, GetPrice(side, 2), OpenCloseHelper.GetOpenCloseString(oc));
            }
        }

        // 重新发单
        private void ResendOrder(Order order)
        {
            SendOrder(order.Side, OpenCloseHelper.CheckOpenClose(order), order.LeavesQty);
        }

        public override void OnOrderPartiallyFilled(Order order)
        {
            dualPosition.Filled(order);
            // 单子部分成交，不做操作，等单子完全执行完
        }

        public override void OnOrderFilled(Order order)
        {
            dualPosition.Filled(order);

            // 检查仓位是否正确,是否要发新单
            Process();
        }

        public override void OnNewOrder(Order order)
        {
            dualPosition.NewOrder(order);
        }

        public override void OnOrderRejected(Order order)
        {
            dualPosition.OrderRejected(order);

            ErrorType et = ParseErrorType.GetError(order.Text);

            double flag = order.Side == OrderSide.Buy ? 1 : -1;

            if (EnumOpenClose.OPEN == OpenCloseHelper.CheckOpenClose(order))
            {
                // 开仓被拒绝，不再新开仓
                TargetPosition -= flag * order.LeavesQty;
                return;
            }

            // 无法平仓，不重发单
            if (et == ErrorType.OVER_CLOSETODAY_POSITION
                || et == ErrorType.OVER_CLOSEYESTERDAY_POSITION
                || et == ErrorType.OVER_CLOSE_POSITION)
            {
                TargetPosition -= flag * order.LeavesQty;
                return;
            }

            // 当前状态禁止此项操作,时间不对，应当等下次操作
        }

        public override void OnOrderCancelled(Order order)
        {
            dualPosition.OrderCancelled(order);

            // 追单
            if (!IsWorkingTime())
            {
                return;
            }

            ResendOrder(order);
        }

        public override void OnOrderCancelReject(Order order)
        {
            // 撤单被拒绝，暂不操作
        }
    }

}

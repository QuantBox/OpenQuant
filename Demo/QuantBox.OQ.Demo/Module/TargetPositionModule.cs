using OpenQuant.API;
using QuantBox.OQ.Demo.Helper;
using QuantBox.OQ.Extensions;
using QuantBox.OQ.Extensions.OrderText;
using System;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetPositionModule : Strategy
    {
        public DualPosition DualPosition;
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
        public double MarketOpenQtyThreshold = 5;
        /// <summary>
        /// 小于等于此数量的平仓单自动以市价发送
        /// </summary>
        public double MarketCloseQtyThreshold = 20;

        public TimeHelper TimeHelper;
        public PriceHelper PriceHelper;

        public override void OnStrategyStart()
        {
            TimeHelper = new TimeHelper(EnumTradingTime.COMMODITY);
            PriceHelper = new PriceHelper(Instrument.TickSize);

            DualPosition = new DualPosition();
            DualPosition.Sell.PriceHelper = PriceHelper;
            DualPosition.Buy.PriceHelper = PriceHelper;

            // 测试代码
            //TargetPosition = 3;
            DualPosition.Long.Qty = 0;
            DualPosition.Short.Qty = 0;
        }

        public override void OnTrade(Trade trade)
        {
            Process();
        }

        public override void OnQuote(Quote quote)
        {
            Process();
        }

        public override void OnBarOpen(Bar bar)
        {
            Process();
        }

        public override void OnBar(Bar bar)
        {
            Process();
        }

        // 最小手续费处理原则
        public void Process()
        {
            // 非交易时段，无法处理
            if (!TimeHelper.IsTradingTime())
            {
                return;
            }

            // 计算仓差
            double dif = TargetPosition - DualPosition.NetQty;
            double qty = 0;
            OrderSide Side = OrderSide.Buy;
            EnumOpenClose oc = EnumOpenClose.OPEN;

            if (dif == 0)// 持仓量相等
            {
                // 把所有的挂单先撤了
            }
            else if (dif > 0 && !DualPosition.IsPending)// 表示要增加净持仓
            {
                // 是否有在途增仓订单,超数了
                // 是否有在途减仓订单,全取消息
                qty = dif;
                Side = OrderSide.Buy;

                if (DualPosition.Short.Qty > 0)
                {
                    // 按最小数量进行平仓
                    qty = Math.Min(qty, DualPosition.Short.Qty);
                    oc = EnumOpenClose.CLOSE;
                }
            }
            else if (!DualPosition.IsPending) // 减少净持仓
            {
                qty = -dif;
                Side = OrderSide.Sell;

                if (DualPosition.Long.Qty > 0)
                {
                    // 按最小数量进行平仓
                    qty = Math.Min(qty, DualPosition.Long.Qty);
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

        // 下单操作
        private void SendOrder(OrderSide side, EnumOpenClose oc, double qty)
        {
            if (!TimeHelper.IsTradingTime())
            {
                return;
            }

            if (qty <= 0)
            {
                return;
            }

            // 为减少滑点，对数量少的单子直接市价单
            bool bMarketOrder = false;
            if (EnumOpenClose.OPEN == oc)
            {
                if (qty <= MarketOpenQtyThreshold)
                    bMarketOrder = true;
            }
            else
            {
                if (qty <= MarketCloseQtyThreshold)
                    bMarketOrder = true;
            }

            if (bMarketOrder)
            {
                SendMarketOrder(side, qty, OpenCloseHelper.GetOpenCloseString(oc));
            }
            else
            {
                SendLimitOrder(side, qty, PriceHelper.GetMatchPrice(this, side, 2), OpenCloseHelper.GetOpenCloseString(oc));
            }
        }

        // 重新发单
        private void ResendOrder(Order order)
        {
            SendOrder(order.Side, OpenCloseHelper.CheckOpenClose(order), order.LeavesQty);
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
            Process();
        }

        public override void OnNewOrder(Order order)
        {
            DualPosition.NewOrder(order);
        }

        public override void OnOrderRejected(Order order)
        {
            EnumOpenClose OpenClose = DualPosition.OrderRejected(order);

            double flag = order.Side == OrderSide.Buy ? 1 : -1;

            if (EnumOpenClose.OPEN == OpenClose)
            {
                // 开仓被拒绝，不再新开仓
                // 有可能是钱不够
                // 有可能是超出持仓限制
                // 有可能是非交易时间
                TargetPosition -= flag * order.LeavesQty;
                return;
            }

            EnumError error = TextResponse.FromText(order.Text);

            // 无法平仓，不重发单
            // 能出现这个问题是持仓计算错误，这已经是策略持仓计算错误了
            if (error == EnumError.OVER_CLOSETODAY_POSITION
                || error == EnumError.OVER_CLOSEYESTERDAY_POSITION
                || error == EnumError.OVER_CLOSE_POSITION)
            {
                TargetPosition -= flag * order.LeavesQty;
                return;
            }

            // 当前状态禁止此项操作,时间不对，应当等下次操作
        }

        public override void OnOrderCancelled(Order order)
        {
            DualPosition.OrderCancelled(order);

            ResendOrder(order);
        }

        public override void OnOrderCancelReject(Order order)
        {
            // 撤单被拒绝，暂不操作
        }
    }

}

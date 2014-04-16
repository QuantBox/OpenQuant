using OpenQuant.API;
using QuantBox.OQ.Extensions;
using System.Collections.Generic;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 非上海的要将平今转成平仓
    /// 上海
    ///     开仓 Qty QtyToday
    ///     平今仓 FrozenCloseToday
    ///     平昨仓 FrozenClose
    /// 非上海
    ///     开仓 Qty
    ///     平仓 FrozenClose
    /// </summary>
    public class DualPosition
    {
        
        public CloseTodayHelper CloseTodayHelper;
        public PositionRecord Long = new PositionRecord();
        public PositionRecord Short = new PositionRecord();

        public OrderBook_OneSide_Order Buy = new OrderBook_OneSide_Order(OrderSide.Buy);
        public OrderBook_OneSide_Order Sell = new OrderBook_OneSide_Order(OrderSide.Sell);

        public Dictionary<Order, EnumOpenClose> Order_OpenClose = new Dictionary<Order, EnumOpenClose>();

        public DualPosition(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; private set; }
        /// <summary>
        /// 换日，用户自己要记得挂单要撤
        /// </summary>
        public void ChangeTradingDay()
        {
            // 挂单要清空，用户得在策略中写上收盘撤单的代码

            // 持仓要移动
            Buy.Clear();
            Sell.Clear();

            Long.ChangeTradingDay();
            Short.ChangeTradingDay();
        }

        public double NetQty
        {
            get { return Long.Qty - Short.Qty; }
        }

        public bool IsPending
        {
            get { return Buy.IsPending || Sell.IsPending; }
        }

        /// <summary>
        /// 产生了新订单
        /// </summary>
        /// <param name="order"></param>
        public void NewOrder(Order order)
        {
            lock (this)
            {
                if (!order.IsPendingNew)
                    return;

                // 非上海的，平今要转成平仓
                EnumOpenClose OpenClose = CloseTodayHelper.Transform(OpenCloseHelper.CheckOpenClose(order));
                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            Long.FrozenOpen += order.Qty;
                        }
                        else
                        {
                            Short.FrozenOpen += order.Qty;
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.FrozenClose += order.Qty;
                        }
                        else
                        {
                            Long.FrozenClose += order.Qty;
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.FrozenCloseToday += order.Qty;
                            Short.FrozenClose += order.Qty;
                        }
                        else
                        {
                            Long.FrozenCloseToday += order.Qty;
                            Long.FrozenClose += order.Qty;
                        }
                        break;
                    default:
                        break;
                }

                if (order.Side == OrderSide.Buy)
                {
                    Buy.Add(order);
                }
                else
                {
                    Sell.Add(order);
                }
                Order_OpenClose[order] = OpenClose;
            }
        }

        /// <summary>
        /// 有订单成交
        /// </summary>
        /// <param name="order"></param>
        public void Filled(Order order)
        {
            lock (this)
            {
                EnumOpenClose OpenClose = EnumOpenClose.OPEN;
                Order_OpenClose.TryGetValue(order, out OpenClose);

                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            Long.Qty += order.LastQty;
                            Long.QtyToday += order.LastQty;
                            Long.FrozenOpen -= order.LastQty;
                            Long.CumOpenQty += order.LastQty;
                            Long.HoldingCost += order.LastPrice * order.LastQty;
                        }
                        else
                        {
                            Short.Qty += order.LastQty;
                            Short.QtyToday += order.LastQty;
                            Short.FrozenOpen -= order.LastQty;
                            Short.CumOpenQty += order.LastQty;
                            Short.HoldingCost += order.LastPrice * order.LastQty;
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.Qty -= order.LastQty;
                            Short.FrozenClose -= order.LastQty;
                            if (Short.Qty == 0)
                            {
                                Short.HoldingCost = 0;
                            }
                            else
                            {
                                Short.HoldingCost -= order.LastPrice * order.LastQty;
                            }
                        }
                        else
                        {
                            Long.Qty -= order.LastQty;
                            Long.FrozenClose -= order.LastQty;
                            if (Long.Qty == 0)
                            {
                                Long.HoldingCost = 0;
                            }
                            else
                            {
                                Long.HoldingCost -= order.LastPrice * order.LastQty;
                            }
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.Qty -= order.LastQty;
                            Short.QtyToday -= order.LastQty;
                            Short.FrozenClose -= order.LastQty;
                            Short.FrozenCloseToday -= order.LastQty;
                            if (Short.Qty == 0)
                            {
                                Short.HoldingCost = 0;
                            }
                            else
                            {
                                Short.HoldingCost -= order.LastPrice * order.LastQty;
                            }
                        }
                        else
                        {
                            Long.Qty -= order.LastQty;
                            Long.QtyToday -= order.LastQty;
                            Long.FrozenClose -= order.LastQty;
                            Long.FrozenCloseToday -= order.LastQty;
                            if (Long.Qty == 0)
                            {
                                Long.HoldingCost = 0;
                            }
                            else
                            {
                                Long.HoldingCost -= order.LastPrice * order.LastQty;
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (order.IsDone)
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Buy.Remove(order);
                    }
                    else
                    {
                        Sell.Remove(order);
                    }
                }
                Order_OpenClose.Remove(order);
            }
        }

        /// <summary>
        /// 单子被撤
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public EnumOpenClose OrderCancelled(Order order)
        {
            lock (this)
            {
                EnumOpenClose OpenClose = EnumOpenClose.OPEN;
                Order_OpenClose.TryGetValue(order, out OpenClose);

                if (PositionSide.Long == OpenCloseHelper.CheckLongShort(order,OpenClose))
                {
                    ++Long.CumCancelCnt;
                }
                else
                {
                    ++Short.CumCancelCnt;
                }

                OrderRejected(order);

                return OpenClose;
            }
        }

        public EnumOpenClose GetOpenClose(Order order)
        {
            EnumOpenClose OpenClose = EnumOpenClose.OPEN;
            Order_OpenClose.TryGetValue(order, out OpenClose);
            return OpenClose;
        }

        public EnumOpenClose OrderRejected(Order order)
        {
            lock (this)
            {
                EnumOpenClose OpenClose = EnumOpenClose.OPEN;
                Order_OpenClose.TryGetValue(order, out OpenClose);

                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            Long.FrozenOpen -= order.LeavesQty;
                        }
                        else
                        {
                            Short.FrozenOpen -= order.LeavesQty;
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.FrozenClose -= order.LeavesQty;
                        }
                        else
                        {
                            Long.FrozenClose -= order.LeavesQty;
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            Short.FrozenCloseToday -= order.LeavesQty;
                            Short.FrozenClose -= order.LeavesQty;
                        }
                        else
                        {
                            Long.FrozenCloseToday -= order.LeavesQty;
                            Long.FrozenClose -= order.LeavesQty;
                        }
                        break;
                    default:
                        break;
                }

                if (order.IsDone)
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Buy.Remove(order);
                    }
                    else
                    {
                        Sell.Remove(order);
                    }
                }
                Order_OpenClose.Remove(order);

                return OpenClose;
            }
        }

        public double Cancel(OrderSide Side)
        {
            lock (this)
            {
                double qty;
                if (Side == OrderSide.Buy)
                {
                    qty = Buy.Cancel();
                }
                else
                {
                    qty = Sell.Cancel();
                }
                return qty;
            }
        }

        public double Cancel()
        {
            lock (this)
            {
                double qty = 0;
                qty += Buy.Cancel();
                qty += Sell.Cancel();
                return qty;
            }
        }

        public override string ToString()
        {
            return Long + " --- " + Short;
        }
    }
}

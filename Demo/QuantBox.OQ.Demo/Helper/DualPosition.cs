using OpenQuant.API;
using QuantBox.OQ.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
        /// <summary>
        /// //平今仓助手
        /// </summary>
        public CloseTodayHelper CloseTodayHelper;
        /// <summary>
        /// //多头仓位记录
        /// </summary>
        public PositionRecord Long = new PositionRecord();
        /// <summary>
        /// //空头仓位记录
        /// </summary>
        public PositionRecord Short = new PositionRecord();

        public OrderBook_OneSide_Order Buy = new OrderBook_OneSide_Order(OrderSide.Buy);
        public OrderBook_OneSide_Order Sell = new OrderBook_OneSide_Order(OrderSide.Sell);
        /// <summary>
        /// 订单的开平仓字典
        /// </summary>
        public Dictionary<Order, EnumOpenClose> Order_OpenClose = new Dictionary<Order, EnumOpenClose>();

        public DualPosition(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; private set; }
        /// <summary>
        /// 多头持仓量
        /// </summary>
        public double LongQty
        {
            get { return Long.Qty; }
        }
        /// <summary>
        /// 空头持仓量
        /// </summary>
        public double ShortQty
        {
            get { return Short.Qty; }
        }
        /// <summary>
        /// 得到买入数量
        /// </summary>
        public double BidQty
        {
            get { return Buy.Size(); }
        }
        /// <summary>
        /// 得到卖出数量
        /// </summary>
        public double AskQty
        {
            get { return Sell.Size(); }
        }
        /// <summary>
        /// 得到买入价位文本信息
        /// </summary>
        public string BidString
        {
            get
            {
                return string.Concat(
                    Buy.SizeByIndex(0),
                    "@",
                    Buy.PriceByIndex(0));
            }
        }
        /// <summary>
        /// 得到卖出价位文本信息
        /// </summary>
        public string AskString
        {
            get
            {
                return string.Concat(
                    Sell.SizeByIndex(0),
                    "@",
                    Sell.PriceByIndex(0));
            }
        }
        /// <summary>
        /// 换日，用户自己要记得挂单要撤
        /// </summary>
        public void ChangeTradingDay()
        {
            // 挂单要清空，用户得在策略中写上收盘撤单的代码

            // 持仓要移动
            lock(this)
            {
                Buy.Clear();
                Sell.Clear();

                Long.ChangeTradingDay();//多头换日
                Short.ChangeTradingDay();//空头换日
            }
        }
        /// <summary>
        /// 实际持仓
        /// </summary>
        public double NetQty
        {
            get { return Long.Qty - Short.Qty; }//多头仓 - 空头仓
        }
        /// <summary>
        /// 是否等待
        /// </summary>
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

                double Qty = order.Qty;

                // 非上海的，平今要转成平仓
                EnumOpenClose OpenClose = CloseTodayHelper.Transform(OpenCloseHelper.CheckOpenClose(order));
                Order_OpenClose[order] = OpenClose;

                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Long.FrozenOpen += Qty;
                            Long.NewOrderOpen(Qty);
                        }
                        else
                        {
                            //Short.FrozenOpen += Qty;
                            Short.NewOrderOpen(Qty);
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.FrozenClose += Qty;
                            Short.NewOrderClose(Qty);
                        }
                        else
                        {
                            //Long.FrozenClose += Qty;
                            Long.NewOrderClose(Qty);
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.FrozenCloseToday += Qty;
                            //Short.FrozenClose += Qty;
                            Short.NewOrderCloseToday(Qty);
                        }
                        else
                        {
                            //Long.FrozenCloseToday += Qty;
                            //Long.FrozenClose += Qty;
                            Long.NewOrderCloseToday(Qty);
                        }
                        break;
                    default:
                        MessageBox.Show("NewOrder");
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
                double LastQty = order.LastQty;//得到最后的填充(部分填充)此订单的数量
                double LastPrice = order.LastPrice;//得到最后的填充(部分填充)此订单价格
                bool IsDone = order.IsDone;//返回true,如果这个订单最终状态(填充,拒绝或取消)

                EnumOpenClose OpenClose = GetOpenClose(order);

                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Long.Qty += LastQty;
                            //Long.QtyToday += LastQty;
                            //Long.FrozenOpen -= LastQty;
                            //Long.CumOpenQty += LastQty;
                            //Long.HoldingCost += LastPrice * LastQty;
                            Long.FilledOpen(LastQty, LastPrice);
                        }
                        else
                        {
                            //Short.Qty += LastQty;
                            //Short.QtyToday += LastQty;
                            //Short.FrozenOpen -= LastQty;
                            //Short.CumOpenQty += LastQty;
                            //Short.HoldingCost += LastPrice * LastQty;
                            Short.FilledOpen(LastQty, LastPrice);
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.Qty -= LastQty;
                            //Short.FrozenClose -= LastQty;
                            //if (Short.Qty == 0)
                            //{
                            //    Short.HoldingCost = 0;
                            //}
                            //else
                            //{
                            //    Short.HoldingCost -= LastPrice * LastQty;
                            //}
                            Short.FilledClose(LastQty, LastPrice);
                        }
                        else
                        {
                            //Long.Qty -= LastQty;
                            //Long.FrozenClose -= LastQty;
                            //if (Long.Qty == 0)
                            //{
                            //    Long.HoldingCost = 0;
                            //}
                            //else
                            //{
                            //    Long.HoldingCost -= LastPrice * LastQty;
                            //}
                            Long.FilledClose(LastQty, LastPrice);
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.Qty -= LastQty;
                            //Short.QtyToday -= LastQty;
                            //Short.FrozenClose -= LastQty;
                            //Short.FrozenCloseToday -= LastQty;
                            //if (Short.Qty == 0)
                            //{
                            //    Short.HoldingCost = 0;
                            //}
                            //else
                            //{
                            //    Short.HoldingCost -= LastPrice * LastQty;
                            //}
                            Short.FilledCloseToday(LastQty, LastPrice);
                        }
                        else
                        {
                            //Long.Qty -= LastQty;
                            //Long.QtyToday -= LastQty;
                            //Long.FrozenClose -= LastQty;
                            //Long.FrozenCloseToday -= LastQty;
                            //if (Long.Qty == 0)
                            //{
                            //    Long.HoldingCost = 0;
                            //}
                            //else
                            //{
                            //    Long.HoldingCost -= LastPrice * LastQty;
                            //}
                            Long.FilledCloseToday(LastQty, LastPrice);
                        }
                        break;
                    default:
                        MessageBox.Show("Filled");
                        break;
                }

                if (IsDone)
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Buy.Remove(order);
                    }
                    else
                    {
                        Sell.Remove(order);
                    }
                    Order_OpenClose.Remove(order);
                }
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
                EnumOpenClose OpenClose = GetOpenClose(order);

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
        /// <summary>
        /// 得到订单的开平标志
        /// </summary>
        /// <param name="order">订单</param>
        /// <returns>返回值：开平仓标记</returns>
        public EnumOpenClose GetOpenClose(Order order)
        {
            EnumOpenClose OpenClose = EnumOpenClose.OPEN;//开平仓标记
            //得到指定order订单的开平仓标记，TryGetValue函数找到时则返回TURE,并返回指定的OpenClose开平仓标记
            if(Order_OpenClose.TryGetValue(order, out OpenClose))
                return OpenClose;
            //没有找到则返回OPEN标记
            return EnumOpenClose.OPEN;
        }
        /// <summary>
        /// 订单被拒绝
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public EnumOpenClose OrderRejected(Order order)
        {
            lock (this)
            {
                double LeavesQty = order.LeavesQty;//得到剩余的订单数量

                EnumOpenClose OpenClose = GetOpenClose(order);//开平标记

                switch (OpenClose)
                {
                    case EnumOpenClose.OPEN:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Long.FrozenOpen -= LeavesQty;
                            Long.OrderRejectedOpen(LeavesQty);
                        }
                        else
                        {
                            //Short.FrozenOpen -= LeavesQty;
                            Short.OrderRejectedOpen(LeavesQty);
                        }
                        break;
                    case EnumOpenClose.CLOSE:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.FrozenClose -= LeavesQty;
                            Short.OrderRejectedClose(LeavesQty);
                        }
                        else
                        {
                            //Long.FrozenClose -= LeavesQty;
                            Long.OrderRejectedClose(LeavesQty);
                        }
                        break;
                    case EnumOpenClose.CLOSE_TODAY:
                        if (order.Side == OrderSide.Buy)
                        {
                            //Short.FrozenCloseToday -= LeavesQty;
                            //Short.FrozenClose -= LeavesQty;
                            Short.OrderRejectedCloseToday(LeavesQty);
                        }
                        else
                        {
                            //Long.FrozenCloseToday -= LeavesQty;
                            //Long.FrozenClose -= LeavesQty;
                            Long.OrderRejectedCloseToday(LeavesQty);
                        }
                        break;
                    default:
                        MessageBox.Show("OrderRejected");
                        break;
                }

                //if (IsDone)
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Buy.Remove(order);
                    }
                    else
                    {
                        Sell.Remove(order);
                    }
                    Order_OpenClose.Remove(order);
                }
                

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

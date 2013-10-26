using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class DualPosition
    {
        public PositionRecord Long = new PositionRecord();
        public PositionRecord Short = new PositionRecord();

        public OrderBook_OneSide_Order Buy = new OrderBook_OneSide_Order(OrderSide.Buy);
        public OrderBook_OneSide_Order Sell = new OrderBook_OneSide_Order(OrderSide.Sell);

        public double NetQty
        {
            get { return Long.Qty - Short.Qty; }
        }

        public bool IsPending
        {
            get { return Buy.IsPending || Sell.IsPending; }
        }


        public void Filled(Order order)
        {
            lock (this)
            {
                if (EnumOpenClose.OPEN == OpenCloseHelper.CheckOpenClose(order))
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Long.Qty += order.LastQty;
                        Long.FrozenOpen -= order.LastQty;
                        Long.CumOpenQty += order.LastQty;
                        Long.HoldingCost += order.LastPrice * order.LastQty;
                    }
                    else
                    {
                        Short.Qty += order.LastQty;
                        Short.FrozenOpen -= order.LastQty;
                        Short.CumOpenQty += order.LastQty;
                        Short.HoldingCost += order.LastPrice * order.LastQty;
                    }
                }
                else
                {
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
            }
        }


        public void NewOrder(Order order)
        {
            lock (this)
            {
                if (!order.IsPendingNew)
                    return;

                if (EnumOpenClose.OPEN == OpenCloseHelper.CheckOpenClose(order))
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Long.FrozenOpen += order.Qty;
                    }
                    else
                    {
                        Short.FrozenOpen += order.Qty;
                    }
                }
                else
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Short.FrozenClose += order.Qty;
                    }
                    else
                    {
                        Long.FrozenClose += order.Qty;
                    }
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


        public void OrderCancelled(Order order)
        {
            lock (this)
            {
                if (PositionSide.Long == OpenCloseHelper.CheckLongShort(order))
                {
                    ++Long.CumCancelCnt;
                }
                else
                {
                    ++Short.CumCancelCnt;
                }


                OrderRejected(order);
            }
        }


        public void OrderRejected(Order order)
        {
            lock (this)
            {
                if (EnumOpenClose.OPEN == OpenCloseHelper.CheckOpenClose(order))
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Long.FrozenOpen -= order.LeavesQty;
                    }
                    else
                    {
                        Short.FrozenOpen -= order.LeavesQty;
                    }
                }
                else
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        Short.FrozenClose -= order.LeavesQty;
                    }
                    else
                    {
                        Long.FrozenClose -= order.LeavesQty;
                    }
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
            }
        }

        public EnumOpenClose CanClose(OrderSide Side, double qty)
        {
            bool bCanClose = false;
            if (Side == OrderSide.Buy)
            {
                bCanClose = Short.CanClose(qty);
            }
            else
            {
                bCanClose = Long.CanClose(qty);
            }
            if (bCanClose)
                return EnumOpenClose.CLOSE;
            return EnumOpenClose.OPEN;
        }

        public override string ToString()
        {
            return Long + " --- " + Short;
        }
    }
}

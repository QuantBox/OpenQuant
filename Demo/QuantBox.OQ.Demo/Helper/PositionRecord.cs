using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 持仓量信息如何保存？
    /// 1.总持仓和今仓。使用此方式，这样在下单时对非上海的可以直接判断
    /// 2.昨持仓和今仓
    /// 
    /// 1.上海与非上海都用一样的保存方法。使用此方式，统一
    /// 2.只上海分昨和今
    /// </summary>
    public class PositionRecord
    {
        /// <summary>
        /// 实际持仓
        /// </summary>
        public double Qty { get; set; }
        /// <summary>
        /// 实际持今仓量
        /// </summary>
        public double QtyToday { get; set; }
        /// <summary>
        /// 挂开仓量
        /// </summary>
        public double FrozenOpen { get; private set; }
        /// <summary>
        /// 挂平仓量
        /// </summary>
        public double FrozenClose { get; private set; }
        /// <summary>
        /// 挂平今量
        /// </summary>
        public double FrozenCloseToday { get; private set; }
        /// <summary>
        /// 开仓手数累计
        /// </summary>
        public double CumOpenQty { get; private set; }
        /// <summary>
        /// 撤单次数累计，注意，不区分主动撤单与被动撤单
        /// </summary>
        public double CumCancelCnt { get; set; }
        /// <summary>
        /// 持仓成本
        /// </summary>
        public double HoldingCost { get; private set; }


        public void Reset()
        {
            Qty = 0;
            HoldingCost = 0;

            ChangeTradingDay();
        }

        public void ChangeTradingDay()
        {
            QtyToday = 0;
            FrozenOpen = 0;
            FrozenClose = 0;
            FrozenCloseToday = 0;
            CumOpenQty = 0;
            CumCancelCnt = 0;
        }

        /// <summary>
        /// 持仓平均成本
        /// </summary>
        public double AvgPrice
        {
            get
            {
                if (Qty == 0)
                    return 0;
                else
                    return HoldingCost / Qty;
            }
        }

        public override string ToString()
        {
            return string.Format("持仓量:{0},挂开仓量:{1},挂平仓量:{2},开仓手数累计:{3}",
                Qty, FrozenOpen, FrozenClose, CumOpenQty);
        }

        public void NewOrderOpen(double Qty)
        {
            FrozenClose += Qty;
        }

        public void NewOrderClose(double Qty)
        {
            FrozenClose += Qty;
        }

        public void NewOrderCloseToday(double Qty)
        {
            FrozenCloseToday += Qty;
            FrozenClose += Qty;
        }

        public void FilledOpen(double LastQty, double LastPrice)
        {
            Qty += LastQty;
            QtyToday += LastQty;
            FrozenOpen -= LastQty;
            CumOpenQty += LastQty;
            HoldingCost += LastPrice * LastQty;
        }

        public void FilledClose(double LastQty, double LastPrice)
        {
            Qty -= LastQty;
            FrozenClose -= LastQty;
            if (Qty == 0)
            {
                HoldingCost = 0;
            }
            else
            {
                HoldingCost -= LastPrice * LastQty;
            }
        }

        public void FilledCloseToday(double LastQty, double LastPrice)
        {
            Qty -= LastQty;
            QtyToday -= LastQty;
            FrozenClose -= LastQty;
            FrozenCloseToday -= LastQty;
            if (Qty == 0)
            {
                HoldingCost = 0;
            }
            else
            {
                HoldingCost -= LastPrice * LastQty;
            }
        }

        public void OrderRejectedOpen(double LeavesQty)
        {
            FrozenOpen -= LeavesQty;
        }

        public void OrderRejectedClose(double LeavesQty)
        {
            FrozenClose -= LeavesQty;
        }

        public void OrderRejectedCloseToday(double LeavesQty)
        {
            FrozenCloseToday -= LeavesQty;
            FrozenClose -= LeavesQty;
        }
    }
}

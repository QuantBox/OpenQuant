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

        /// <summary>
        /// 重新初始化
        /// </summary>
        public void Reset()
        {
            Qty = 0;//实际持仓量
            HoldingCost = 0;//持仓成本

            ChangeTradingDay();
        }

        /// <summary>
        /// 换日，用户自己要记得挂单要撤
        /// </summary>
        public void ChangeTradingDay()
        {
            QtyToday = 0;//今实际持仓量=0
            FrozenOpen = 0;//挂开仓量=0
            FrozenClose = 0;//挂平仓量=0
            FrozenCloseToday = 0;//挂平今量=0
            CumOpenQty = 0;//开仓手数累计=0
            CumCancelCnt = 0;//撤单次数累计=0
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
                    return HoldingCost / Qty;//持仓成本/持仓量
            }
        }
        /// <summary>
        /// 转换持仓量信息为文本信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("持仓量:{0},挂开仓量:{1},挂平仓量:{2},开仓手数累计:{3}",
                Qty, FrozenOpen, FrozenClose, CumOpenQty);
        }
        /// <summary>
        /// 新增开仓订单
        /// </summary>
        /// <param name="Qty">开仓数量</param>
        public void NewOrderOpen(double Qty)
        {
            FrozenClose += Qty;//挂平仓量+开仓手数
        }
        /// <summary>
        /// 新增平仓订单
        /// </summary>
        /// <param name="Qty">平仓数量</param>
        public void NewOrderClose(double Qty)
        {
            FrozenClose += Qty;//挂平仓量+开仓手数
        }
        /// <summary>
        /// 新增平今订单
        /// </summary>
        /// <param name="Qty">平今数量</param>
        public void NewOrderCloseToday(double Qty)
        {
            FrozenCloseToday += Qty;//挂平今数量变动
            FrozenClose += Qty;//挂平仓量变动
        }
        /// <summary>
        /// 填充开仓
        /// </summary>
        /// <param name="LastQty"></param>
        /// <param name="LastPrice"></param>
        public void FilledOpen(double LastQty, double LastPrice)
        {
            Qty += LastQty;
            QtyToday += LastQty;
            FrozenOpen -= LastQty;
            CumOpenQty += LastQty;
            HoldingCost += LastPrice * LastQty;
        }
        /// <summary>
        /// 填充平仓
        /// </summary>
        /// <param name="LastQty"></param>
        /// <param name="LastPrice"></param>
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
        /// <summary>
        /// 填充平今仓
        /// </summary>
        /// <param name="LastQty"></param>
        /// <param name="LastPrice"></param>
        public void FilledCloseToday(double LastQty, double LastPrice)
        {
            Qty -= LastQty;// 实际持仓量 - 平今仓订单的数量 
            QtyToday -= LastQty;//实际持今仓量  - 平今仓订单的数量 
            FrozenClose -= LastQty;//挂平仓量 - 平今仓订单的数量 
            FrozenCloseToday -= LastQty;//挂平今量 - 平今仓订单的数量 
            //重新计算持仓成本
            if (Qty == 0)
            {
                HoldingCost = 0;
            }
            else
            {
                HoldingCost -= LastPrice * LastQty;//减去平仓的成本
            }
        }
        /// <summary>
        /// 拒绝订单开仓
        /// </summary>
        /// <param name="LeavesQty"></param>
        public void OrderRejectedOpen(double LeavesQty)
        {
            FrozenOpen -= LeavesQty;
        }
        /// <summary>
        /// 拒绝订单平仓
        /// </summary>
        /// <param name="LeavesQty"></param>
        public void OrderRejectedClose(double LeavesQty)
        {
            FrozenClose -= LeavesQty;
        }
        /// <summary>
        /// 拒绝订单平今仓
        /// </summary>
        /// <param name="LeavesQty"></param>
        public void OrderRejectedCloseToday(double LeavesQty)
        {
            FrozenCloseToday -= LeavesQty;
            FrozenClose -= LeavesQty;
        }
    }
}

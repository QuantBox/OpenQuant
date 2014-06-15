using QuantBox.OQ.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 平今仓助手，上交所有平令仓和平昨仓区别
    /// </summary>
    public class CloseTodayHelper
    {
        /// <summary>
        /// 是否上交所
        /// </summary>
        public bool IsSHFE { get; private set; }
        /// <summary>
        /// 是否平今仓
        /// </summary>
        /// <param name="exchange"></param>
        public CloseTodayHelper(EnumExchangeID exchange)
        {
            this.IsSHFE = (exchange == EnumExchangeID.SHFE);
        }

        public CloseTodayHelper(bool IsSHFE)
        {
            this.IsSHFE = IsSHFE;
        }
        /// <summary>
        /// 平今仓转换成平昨仓标志
        /// </summary>
        /// <param name="OpenClose"></param>
        /// <returns></returns>
        public EnumOpenClose Transform(EnumOpenClose OpenClose)
        {
            if (!IsSHFE)
            {
                if (OpenClose == EnumOpenClose.CLOSE_TODAY)
                    return EnumOpenClose.CLOSE;
            }
            return OpenClose;
        }
        /// <summary>
        /// 计算平昨仓和平
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="OpenClose"></param>
        /// <returns></returns>
        public double GetCloseAndQty(PositionRecord pos,out EnumOpenClose OpenClose)
        {
            double qty = 0;//可平仓量
            OpenClose = EnumOpenClose.CLOSE;//开平标志，默认设置为平昨仓
            if (IsSHFE)//是否上交所
            {
                // 上海，先检查今仓可平量，今仓可平量=实际今仓量-挂平今量
                qty = pos.QtyToday - pos.FrozenCloseToday;
                if (qty > 0)//今仓可平量>0时
                {
                    OpenClose = EnumOpenClose.CLOSE_TODAY;//设置平今标志
                }
                else//今仓可平量==0时
                {
                    // 计算出昨可平仓量，再查挂的昨平仓有多少，昨仓=（实际持仓-实际今仓）-（挂平仓量 - 挂平今量）
                    qty = (pos.Qty - pos.QtyToday) - (pos.FrozenClose - pos.FrozenCloseToday);
                }
            }
            else
            {
                // 非上海，直接返回
                qty = pos.Qty - pos.FrozenClose;
            }
            return qty;
        }
    }
}

using QuantBox.OQ.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class CloseTodayHelper
    {
        public bool IsSHFE { get; private set; }

        public CloseTodayHelper(EnumExchangeID exchange)
        {
            this.IsSHFE = (exchange == EnumExchangeID.SHFE);
        }

        public CloseTodayHelper(bool IsSHFE)
        {
            this.IsSHFE = IsSHFE;
        }

        public EnumOpenClose Transform(EnumOpenClose OpenClose)
        {
            if (!IsSHFE)
            {
                if (OpenClose == EnumOpenClose.CLOSE_TODAY)
                    return EnumOpenClose.CLOSE;
            }
            return OpenClose;
        }

        public double GetCloseAndQty(PositionRecord pos,out EnumOpenClose OpenClose)
        {
            double qty = 0;
            OpenClose = EnumOpenClose.CLOSE;
            if (IsSHFE)
            {
                // 上海，先检查今仓
                qty = pos.QtyToday - pos.FrozenCloseToday;
                if (qty > 0)
                {
                    OpenClose = EnumOpenClose.CLOSE_TODAY;
                }
                else
                {
                    // 先算出昨仓，再查挂的昨平仓有多少
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

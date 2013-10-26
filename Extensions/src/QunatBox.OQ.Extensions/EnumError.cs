using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions
{
    public enum EnumError
    {
        /// <summary>
        /// 没有预定义的错误
        /// </summary>
        OTHER = -1,
        /// <summary>
        /// 正确
        /// </summary>
        SUCCESS,
        /// <summary>
        /// 资金不足
        /// </summary>
        INSUFFICIENT_MONEY,
        /// <summary>
        /// 平仓量超过持仓量
        /// </summary>
        OVER_CLOSE_POSITION,
        /// <summary>
        /// 平今仓位不足
        /// </summary>
        OVER_CLOSETODAY_POSITION,
        /// <summary>
        /// 平今仓位不足
        /// </summary>
        OVER_CLOSEYESTERDAY_POSITION,

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions
{
    public enum EnumGroupType
    {
        /// <summary>
        /// 普通单
        /// </summary>
        COMMON,
        /// <summary>
        /// 做市商双向报价
        /// </summary>
        QUOTE,
        /// <summary>
        /// 跨期套利
        /// </summary>
        SP,
        /// <summary>
        /// 跨品种套利
        /// </summary>
        SPC
    }
}

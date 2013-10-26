using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.OrderText
{
    /// <summary>
    /// 跨期套利
    /// </summary>
    public class TextSP : TextRequest
    {
        // 本想使用合约名，后来一想，由系统自行拼接更好，用户少写一个参数
        //public string Symbol;


        public TextSP()
            : base()
        {
            base.Type = EnumGroupType.SP;
        }
    }
}

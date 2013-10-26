using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.OrderText
{
    /// <summary>
    /// 跨期套利
    /// </summary>
    public class TextSP : TextCommon
    {
        public TextSP()
            : base()
        {
            base.Type = EnumGroupType.SP;
        }

        [JsonIgnore]
        public static readonly string Open = new TextSP() { OpenClose = EnumOpenClose.OPEN }.ToString();
        [JsonIgnore]
        public static readonly string Close = new TextSP() { OpenClose = EnumOpenClose.CLOSE }.ToString();
        [JsonIgnore]
        public static readonly string CloseToday = new TextSP() { OpenClose = EnumOpenClose.CLOSE_TODAY }.ToString();
    }
}

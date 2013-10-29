using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.OrderText
{
    /// <summary>
    /// 跨品种套利
    /// </summary>
    public class TextSPC : TextSP
    {
        public TextSPC()
            : base()
        {
            base.Type = EnumGroupType.SPC;
        }

        [JsonIgnore]
        public static readonly string Open = new TextSPC() { OpenClose = EnumOpenClose.OPEN }.ToString();
        [JsonIgnore]
        public static readonly string Close = new TextSPC() { OpenClose = EnumOpenClose.CLOSE }.ToString();
        [JsonIgnore]
        public static readonly string CloseToday = new TextSPC() { OpenClose = EnumOpenClose.CLOSE_TODAY }.ToString();
    }
}

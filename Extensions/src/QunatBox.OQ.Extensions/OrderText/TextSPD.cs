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
    public class TextSPD : TextSP
    {
        public TextSPD()
            : base()
        {
            base.Type = EnumGroupType.SPD;
        }

        [JsonIgnore]
        public static readonly string Open = new TextSPD() { OpenClose = EnumOpenClose.OPEN }.ToString();
        [JsonIgnore]
        public static readonly string Close = new TextSPD() { OpenClose = EnumOpenClose.CLOSE }.ToString();
        [JsonIgnore]
        public static readonly string CloseToday = new TextSPD() { OpenClose = EnumOpenClose.CLOSE_TODAY }.ToString();
    }
}

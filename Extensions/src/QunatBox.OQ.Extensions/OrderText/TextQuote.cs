using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.OrderText
{
    /// <summary>
    /// 做市商双向报价参数
    /// </summary>
    public class TextQuote : TextCommon
    {
        /// <summary>
        /// 报价ID,从交易所的询价请求中取得
        /// </summary>
        public string QuoteID;

        public int StayTime;

        public TextQuote():base()
        {
            base.Type = EnumGroupType.QUOTE;
        }

        [JsonIgnore]
        public static readonly string Open = new TextQuote() { OpenClose = EnumOpenClose.OPEN }.ToString();
        [JsonIgnore]
        public static readonly string Close = new TextQuote() { OpenClose = EnumOpenClose.CLOSE }.ToString();
        [JsonIgnore]
        public static readonly string CloseToday = new TextQuote() { OpenClose = EnumOpenClose.CLOSE_TODAY }.ToString();
    }
}

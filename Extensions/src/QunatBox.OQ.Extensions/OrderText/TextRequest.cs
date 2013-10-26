using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;

namespace QuantBox.OQ.Extensions.OrderText
{
    public class TextCommon : TextParameter
    {
        /// <summary>
        /// 开平标记
        /// </summary>
        [DefaultValue(EnumOpenClose.NONE)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumOpenClose OpenClose;
        /// <summary>
        /// 用户自定的的文本域
        /// </summary>
        public string Text;

        [JsonIgnore]
        public static readonly string Open = new TextCommon() { OpenClose = EnumOpenClose.OPEN }.ToString();
        [JsonIgnore]
        public static readonly string Close = new TextCommon() { OpenClose = EnumOpenClose.CLOSE }.ToString();
        [JsonIgnore]
        public static readonly string CloseToday = new TextCommon() { OpenClose = EnumOpenClose.CLOSE_TODAY }.ToString();
    }
}

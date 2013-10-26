using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace QuantBox.OQ.Extensions.OrderText
{
    public class TextRequest : TextParameter
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
    }
}

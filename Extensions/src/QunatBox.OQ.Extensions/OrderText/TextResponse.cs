
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace QuantBox.OQ.Extensions.OrderText
{
    public class TextResponse : TextCommon
    {
        /// <summary>
        /// 错误类型
        /// </summary>
        public EnumError Error;
        /// <summary>
        /// 底层错误ID
        /// </summary>
        public int ErrorID;
        /// <summary>
        /// 底层错误信息
        /// </summary>
        public string ErrorMsg;
        /// <summary>
        /// 底层状态信息
        /// </summary>
        public string StatusMsg;

        public static EnumError FromText(string text)
        {
            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                TextResponse parameter = JsonConvert.DeserializeObject<TextResponse>(text);
                return parameter.Error;
            }
            return EnumError.SUCCESS;
        }
    }
}

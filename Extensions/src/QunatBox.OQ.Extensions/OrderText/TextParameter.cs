using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.OrderText
{
    public class TextParameter
    {
        /// <summary>
        /// 组类别
        /// </summary>
        [DefaultValue(EnumGroupType.COMMON)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumGroupType Type;

        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, jSetting);
        }
    }
}

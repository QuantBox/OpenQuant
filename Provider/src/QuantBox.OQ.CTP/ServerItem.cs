using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace QuantBox.OQ.CTP
{
    [DefaultPropertyAttribute("Label")]
    public class ServerItem
    {
        private const string OPEN_QUANT = "OpenQuant";
        private string userProductInfo = OPEN_QUANT;      

        [CategoryAttribute("客户端认证"),
        DescriptionAttribute("区别于其它客户端的标识"),
        DefaultValue(OPEN_QUANT)]
        public string UserProductInfo
        {
            get { return userProductInfo; }
            set { userProductInfo = value; }
        }

        [CategoryAttribute("客户端认证"),
        DescriptionAttribute("如果不需要认证此处一定要置为空")]
        public string AuthCode
        {
            get;
            set;
        }

        [CategoryAttribute("服务端信息")]
        public string BrokerID
        {
            get;
            set;
        }

        private BindingList<string> trading = new BindingList<string>();
        [CategoryAttribute("服务端信息"),
        DescriptionAttribute("交易服务器地址")]
        public BindingList<string> Trading
        {
            get{ return trading; }
            set{ trading = value;}
        }

        private BindingList<string> marketData = new BindingList<string>();
        [CategoryAttribute("服务端信息"),
        DescriptionAttribute("行情服务器地址")]
        public BindingList<string> MarketData
        {
            get { return marketData; }
            set { marketData = value; }
        }

        [CategoryAttribute("标签"),
        DescriptionAttribute("标签不能重复")]
        public string Label
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "标签不能重复";
        }

        [BrowsableAttribute(false)]
        public string Name
        {
            get { return Label; }
        }

        public ServerItem()
        {
            marketData.ListChanged += new ListChangedEventHandler(Settings_ListChanged);
            trading.ListChanged += new ListChangedEventHandler(Settings_ListChanged);
        }
        
        public event EventHandler Changed;

        void Settings_ListChanged(object sender, ListChangedEventArgs e)
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}

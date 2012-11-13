using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using QuantBox.CSharp2C;
using SmartQuant;
using SmartQuant.Providers;

namespace QuantBox.OQ.CTP
{
    partial class QBProvider
    {
        private const string CATEGORY_ACCOUNT = "Account";
        private const string CATEGORY_BARFACTORY = "Bar Factory";
        private const string CATEGORY_DEBUG = "Debug";
        private const string CATEGORY_EXECUTION = "Settings - Execution";
        private const string CATEGORY_HISTORICAL = "Settings - Historical Data";
        private const string CATEGORY_INFO = "Information";
        private const string CATEGORY_NETWORK = "Settings - Network";
        private const string CATEGORY_STATUS = "Status";

        //交易所常量定义
        private enum ExchangID
        {
            SHFE,
            DCE,
            CZCE,
            CFFEX
        }

        public enum TimeMode
        {
            LocalTime,
            ExchangeTime
        }

        public enum SetTimeMode
        {
            None,
            LoginTime,
            SHFETime,
            DCETime,
            CZCETime,
            FFEXTime
        }

        private const string OpenPrefix = "O|";
        private const string ClosePrefix = "C|";
        private const string CloseTodayPrefix = "T|";
        private const string CloseYesterdayPrefix = "Y|";

        #region 参数设置
        private string _ApiTempPath;
        private bool bOutputLog;
        private TimeMode _TimeMode;
        private THOST_TE_RESUME_TYPE _ResumeType;
        private string _SupportMarketOrder;
        private string _SupportCloseToday;
        private string _DefaultOpenClosePrefix;

        [Category("Settings - Other")]
        [Description("设置API生成临时文件的目录")]
        [Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Browsable(false)]
        public string ApiTempPath
        {
            get { return _ApiTempPath; }
            set { _ApiTempPath = value; }
        }

        [Category("Settings - Other")]
        [Description("是否输出日志到控制台")]
        [DefaultValue(true)]
        public bool OutputLog
        {
            get { return bOutputLog; }
            set { bOutputLog = value; }
        }

        [Category("Settings - Time")]
        [Description("警告！仅保存行情数据时才用交易所时间。交易时使用交易所时间将导致Bar生成错误")]
        [DefaultValue(TimeMode.LocalTime)]
        public TimeMode DateTimeMode
        {
            get { return _TimeMode; }
            set { _TimeMode = value; }
        }

        [Category("Settings - Time")]
        [Description("修改本地时间。分别是：不修改、登录交易前置机时间、各大交易所时间。以管理员方式运行才有权限")]
        [DefaultValue(SetTimeMode.None)]
        public SetTimeMode SetLocalTimeMode
        {
            get;
            set;
        }

        [Category("Settings - Time")]
        [DefaultValue(0)]
        [Description("修改本地时间时，在取到的时间上添加指定毫秒")]
        public int AddMilliseconds
        {
            get;
            set;
        }

        [Category("Settings - Other")]
        [Description("设置登录后是否接收完整的报单和成交记录")]
        [DefaultValue(THOST_TE_RESUME_TYPE.THOST_TERT_QUICK)]
        public THOST_TE_RESUME_TYPE ResumeType
        {
            get { return _ResumeType; }
            set { _ResumeType = value; }
        }

        [Category("Settings - Other")]
        [Description("设置投机套保标志。Speculation:投机、Arbitrage套利、Hedge套保")]
        [DefaultValue(TThostFtdcHedgeFlagType.Speculation)]
        public TThostFtdcHedgeFlagType HedgeFlagType
        {
            get;
            set;
        }

        [Category("Settings - Other")]
        [Description("支持市价单的交易所")]
        public string SupportMarketOrder
        {
            get { return _SupportMarketOrder; }
        }


        [Category("Settings - Other")]
        [Description("区分平今与平昨的交易所")]
        public string SupportCloseToday
        {
            get { return _SupportCloseToday; }
        }

        [Category("Settings - Other")]
        [Description("指定开平，利用Order的Text域开始部分指定开平，“O|”开仓；“C|”智能平仓；“T|”平今仓；“Y|”平昨仓；")]
        public string DefaultOpenClosePrefix
        {
            get { return _DefaultOpenClosePrefix; }
        }

        private BindingList<ServerItem> serversList = new BindingList<ServerItem>();
        [CategoryAttribute("Settings")]
        [Description("服务器信息，只选择第一条登录")]
        public BindingList<ServerItem> Server
        {
            get { return serversList; }
            set { serversList = value; }
        }

        private BindingList<AccountItem> accountsList = new BindingList<AccountItem>();
        [CategoryAttribute("Settings")]
        [Description("账号信息，只选择第一条登录")]
        public BindingList<AccountItem> Account
        {
            get { return accountsList; }
            set { accountsList = value; }
        }

        [CategoryAttribute("Settings")]
        [Description("插件版本信息")]
        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        [CategoryAttribute("Settings")]
        [Description("连接到行情。此插件不连接行情时底层对不支持市价的报单不会做涨跌停修正，需策略层处理")]
        [DefaultValue(true)]
        public bool ConnectToMarketData
        {
            get { return _bWantMdConnect; }
            set { _bWantMdConnect = value; }
        }

        [CategoryAttribute("Settings")]
        [Description("连接到交易")]
        [DefaultValue(true)]
        public bool ConnectToTrading
        {
            get { return _bWantTdConnect; }
            set { _bWantTdConnect = value; }
        }

        #endregion
        private void InitSettings()
        {
            ApiTempPath = Framework.Installation.TempDir.FullName;
            OutputLog = true;
            ResumeType = THOST_TE_RESUME_TYPE.THOST_TERT_QUICK;
            HedgeFlagType = TThostFtdcHedgeFlagType.Speculation;

            _bWantMdConnect = true;
            _bWantTdConnect = true;

            _SupportMarketOrder = ExchangID.DCE.ToString() + ";" + ExchangID.CZCE.ToString() + ";" + ExchangID.CFFEX.ToString() + ";";
            _SupportCloseToday = ExchangID.SHFE.ToString() + ";";
            _DefaultOpenClosePrefix = OpenPrefix + ";" + ClosePrefix+";"+CloseTodayPrefix + ";" + CloseYesterdayPrefix;

            serversList.ListChanged += new ListChangedEventHandler(ServersList_ListChanged);
            accountsList.ListChanged += new ListChangedEventHandler(AccountsList_ListChanged);

            LoadAccounts();
            LoadServers();
        }

        void ServersList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded) {
                serversList[e.NewIndex].Changed += new EventHandler(ServerItem_ListChanged);
            }
            SettingsChanged();
        }

        void AccountsList_ListChanged(object sender, EventArgs e)
        {
            SettingsChanged();
        }

        void ServerItem_ListChanged(object sender, EventArgs e)
        {
            SettingsChanged();
        }        

        private System.Timers.Timer timerSettingsChanged = new System.Timers.Timer(10000);
        void SettingsChanged()
        {
            //发现会多次触发，想法减少频率才好
            if (false == timerSettingsChanged.Enabled)
            {
                timerSettingsChanged.Elapsed += new System.Timers.ElapsedEventHandler(timerSettingsChanged_Elapsed);
                timerSettingsChanged.AutoReset = false;
            }
            //将上次已经开始的停掉
            timerSettingsChanged.Enabled = false;
            timerSettingsChanged.Enabled = true;
        }

        void timerSettingsChanged_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveAccounts();
            SaveServers();

            timerSettingsChanged.Elapsed -= new System.Timers.ElapsedEventHandler(timerSettingsChanged_Elapsed);
        }

        private string accountsFile = string.Format(@"{0}\CTP.Accounts.xml", Framework.Installation.IniDir);
        void LoadAccounts()
        {
            try
            {
                var accounts = from c in XElement.Load(accountsFile).Elements("Account")
                               select c;
                foreach (var account in accounts)
                {
                    AccountItem ai = new AccountItem();
                    ai.Label = account.Attribute("Label").Value;
                    ai.InvestorId = account.Attribute("InvestorId").Value;
                    ai.Password = account.Attribute("Password").Value;
                    accountsList.Add(ai);
                }
            }
            catch (Exception)
            {
            }            
        }

        void SaveAccounts()
        {
            XElement root = new XElement("Accounts");
            foreach (var account in accountsList)
            {
                XElement acc = new XElement("Account");
                acc.SetAttributeValue("Label", string.IsNullOrEmpty(account.Label) ? "" : account.Label);
                acc.SetAttributeValue("InvestorId", string.IsNullOrEmpty(account.InvestorId) ? "" : account.InvestorId);
                acc.SetAttributeValue("Password", string.IsNullOrEmpty(account.Password) ? "" : account.Password);
                root.Add(acc);
            }
            root.Save(accountsFile);
        }

        private string serversFile = string.Format(@"{0}\CTP.Servers.xml", Framework.Installation.IniDir);
        void LoadServers()
        {
            try
            {
                var servers = from c in XElement.Load(serversFile).Elements("Server")
                          select c;

                foreach (var server in servers)
                {
                    ServerItem si = new ServerItem();
                    si.Label = server.Attribute("Label").Value;
                    si.BrokerID = server.Attribute("BrokerID").Value;
                    si.UserProductInfo = server.Attribute("UserProductInfo").Value;
                    si.AuthCode = server.Attribute("AuthCode").Value;

                    string[] tdarr = server.Attribute("Trading").Value.Split(';');
                    foreach (string s in tdarr)
                    {
                        if (!string.IsNullOrEmpty(s))
                            si.Trading.Add(s);
                    }

                    string[] mdarr = server.Attribute("MarketData").Value.Split(';');
                    foreach (string s in mdarr)
                    {
                        if (!string.IsNullOrEmpty(s))
                            si.MarketData.Add(s);
                    }

                    serversList.Add(si);
                }
            }
            catch (Exception)
            {
            }
        }

        void SaveServers()
        {
            XElement root = new XElement("Servers");
            foreach (var server in serversList)
            {
                XElement ser = new XElement("Server");
                ser.SetAttributeValue("Label", string.IsNullOrEmpty(server.Label) ? "" : server.Label);
                ser.SetAttributeValue("BrokerID", string.IsNullOrEmpty(server.BrokerID) ? "" : server.BrokerID);
                ser.SetAttributeValue("UserProductInfo", string.IsNullOrEmpty(server.UserProductInfo) ? "" : server.UserProductInfo);
                ser.SetAttributeValue("AuthCode", string.IsNullOrEmpty(server.AuthCode) ? "" : server.AuthCode);

                string tdstr = string.Join(";", server.Trading.ToArray());
                ser.SetAttributeValue("Trading", string.IsNullOrEmpty(tdstr) ? "" : tdstr);

                string mdstr = string.Join(";", server.MarketData.ToArray());
                ser.SetAttributeValue("MarketData", string.IsNullOrEmpty(mdstr) ? "" : mdstr);

                root.Add(ser);
            }
            root.Save(serversFile);
        }
    }
}

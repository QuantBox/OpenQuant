using System;
using System.ComponentModel;
using SmartQuant;
using SmartQuant.Providers;

namespace QuantBox.OQ.CTP
{
    public partial class QBProvider : IProvider
    {
        private ProviderStatus status;
        private bool isConnected;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event ProviderErrorEventHandler Error;

        public QBProvider()
        {
            timerConnect.Elapsed += new System.Timers.ElapsedEventHandler(timerConnect_Elapsed);
            timerAccount.Elapsed += new System.Timers.ElapsedEventHandler(timerAccount_Elapsed);
            timerPonstion.Elapsed += new System.Timers.ElapsedEventHandler(timerPonstion_Elapsed);

            InitCallbacks();
            InitSettings();

            BarFactory = new BarFactory();
            status = ProviderStatus.Unknown;
            ProviderManager.Add(this);
        }

        #region IProvider
        [Category(CATEGORY_INFO)]
        public byte Id
        {
            get { return 55; }//不能与已经安装的插件ID重复
        }

        [Category(CATEGORY_INFO)]
        public string Name
        {
            get { return "CTP"; }//不能与已经安装的插件Name重复
        }

        [Category(CATEGORY_INFO)]
        public string Title
        {
            get { return "QuantBox CTP Provider"; }
        }

        [Category(CATEGORY_INFO)]
        public string URL
        {
            get { return "www.quantbox.cn"; }
        }

        public void Connect(int timeout)
        {
            Connect();
            ProviderManager.WaitConnected(this, timeout);
        }

        public void Connect()
        {
            _Connect();
        }

        public void Disconnect()
        {
            _Disconnect(false);
        }

        public void Shutdown()
        {
            Disconnect();
            //特殊的地方,有可能改动了配置就直接关了，还没等保存，所以这地方得保存下
            if (timerSettingsChanged.Enabled)
            {
                SaveAccounts();
                SaveServers();
            }

            timerConnect.Elapsed -= new System.Timers.ElapsedEventHandler(timerConnect_Elapsed);
            timerAccount.Elapsed -= new System.Timers.ElapsedEventHandler(timerAccount_Elapsed);
            timerPonstion.Elapsed -= new System.Timers.ElapsedEventHandler(timerPonstion_Elapsed);
        }

        [Category(CATEGORY_STATUS)]
        public bool IsConnected
        {
            get { return isConnected; }
        }

        [Category(CATEGORY_STATUS)]
        public ProviderStatus Status
        {
            get { return status; }
        }

        public event EventHandler StatusChanged;

        private void ChangeStatus(ProviderStatus status)
        {
            this.status = status;
            EmitStatusChangedEvent();
        }

        private void EmitStatusChangedEvent()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        private void EmitConnectedEvent()
        {
            isConnected = true;
            if (Connected != null)
            {
                Connected(this, EventArgs.Empty);
            }
        }

        private void EmitDisconnectedEvent()
        {
            isConnected = false;
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
            }
        }

        private void EmitError(int id, int code, string message)
        {
            if (Error != null)
                Error(new ProviderErrorEventArgs(new ProviderError(Clock.Now, this, id, code, message)));
        }
        #endregion
    }
}

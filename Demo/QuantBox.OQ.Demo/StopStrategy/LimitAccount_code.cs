using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.StopStrategy
{
    /// <summary>
    /// 限制指定账号使用
    /// </summary>
    public class LimitAccount_code : Strategy
    {
        bool CheckAccount()
        {
            BrokerInfo bi = DataManager.GetBrokerInfo();
            return bi.Accounts[0].Name.CompareTo("你的账号") == 0;
        }

        public override void OnPositionOpened()
        {
            if (!CheckAccount())
                StopStrategy();
        }

        public override void OnBar(Bar bar)
        {
            if (!CheckAccount())
                StopStrategy();
        }
    }
}

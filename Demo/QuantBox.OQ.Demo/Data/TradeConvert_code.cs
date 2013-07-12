using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;

using QuantBox.CSharp2CTP;
using QuantBox.Helper.CTP;

namespace QuantBox.OQ.Demo.Data
{
    public class TradeConvert_code:Strategy
    {
        public override void OnTrade(Trade trade)
        {
            // 插件3.8.2.0 中开始可用，是将Trade数据中的深度数据取出
            CThostFtdcDepthMarketDataField DepthMarketData;
            if (TradeConvert.TryConvert(trade, out DepthMarketData))
            {
                Console.WriteLine(DepthMarketData.LastPrice);
                Console.WriteLine(DepthMarketData.UpperLimitPrice);
            }
        }
    }
}

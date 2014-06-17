using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;

using QuantBox.CSharp2CTP;
using QuantBox.Helper.CTP;

namespace QuantBox.OQ.Demo.Data
{
    public class DataConvert_code:Strategy
    {
        CThostFtdcDepthMarketDataField DepthMarketData;

        public override void OnTrade(Trade trade)
        {
            // 插件3.8.2.0 中开始可用，是将Trade数据中的深度数据取出
            if (DataConvert.TryConvert(trade, ref DepthMarketData))
            {
                Console.WriteLine("OnTrade " + DepthMarketData.LastPrice);
                Console.WriteLine("OnTrade " + DepthMarketData.UpperLimitPrice);
            }
        }

        public override void OnQuote(Quote quote)
        {
            // 插件3.8.2.1 中开始可用，是将Quote数据中的深度数据取出
            if (DataConvert.TryConvert(quote, ref DepthMarketData))
            {
                Console.WriteLine("OnQuote " + DepthMarketData.LastPrice);
                Console.WriteLine("OnQuote " + DepthMarketData.UpperLimitPrice);
            }
        }
    }
}

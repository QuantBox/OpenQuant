using QuantBox.Helper.CTP;
using QuantBox.CSharp2CTP;
using System.Collections.Generic;

namespace QuantBox.OQ.Demo.Plugins_Functions
{
    /// <summary>
    /// 生成多个价差的示例
    /// </summary>
    
    /* 此方法已经过期，在插件3.8.2.0中开始废弃
     * 
    public class MySpreadMarketData : ISpreadMarketData
    {
        private Dictionary<string, CThostFtdcDepthMarketDataField> _dict = new Dictionary<string, CThostFtdcDepthMarketDataField>();

        public IEnumerable<Tick> CalculateSpread(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            List<Tick> list = new List<Tick>();
            _dict[pDepthMarketData.InstrumentID] = pDepthMarketData;

            if (pDepthMarketData.InstrumentID == "IF1305"
                    || pDepthMarketData.InstrumentID == "IF1306")
            {
                CThostFtdcDepthMarketDataField pMD1, pMD2;
                if (_dict.TryGetValue("IF1305", out pMD1)
                        && _dict.TryGetValue("IF1306", out pMD2))
                {
                    Tick t1 = new Tick("IF1305-IF1306",
                            pMD1.LastPrice - pMD2.LastPrice,
                            0);

                    Tick t2 = new Tick("IF1305-IF1306*2",
                            pMD1.LastPrice - pMD2.LastPrice * 2,
                            0);

                    list.Add(t1);
                    list.Add(t2);
                }
            }

            return list;
        }
    }


    /// <summary>
    /// 使用ProviderId区分是哪个合约行情到来时触发示例
    /// </summary>
    public class MySpreadMarketData2 : ISpreadMarketData
    {
        private Dictionary<string, CThostFtdcDepthMarketDataField> _dict = new Dictionary<string, CThostFtdcDepthMarketDataField>();

        public IEnumerable<Tick> CalculateSpread(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            List<Tick> list = new List<Tick>();
            _dict[pDepthMarketData.InstrumentID] = pDepthMarketData;

            byte ProviderId = 255;

            if (pDepthMarketData.InstrumentID == "IF1305")
            {
                ProviderId = 1;
            }
            else if (pDepthMarketData.InstrumentID == "IF1306")
            {
                ProviderId = 2;
            }

            if (ProviderId != 255)
            {
                CThostFtdcDepthMarketDataField pMD1, pMD2;
                if (_dict.TryGetValue("IF1305", out pMD1)
                        && _dict.TryGetValue("IF1306", out pMD2))
                {
                    Tick t1 = new Tick("IF1305-IF1306",
                            pMD1.LastPrice - pMD2.LastPrice,
                            0,
                            ProviderId);

                    Tick t2 = new Tick("IF1305-IF1306*2",
                            pMD1.LastPrice - pMD2.LastPrice * 2,
                            0,
                            ProviderId);

                    list.Add(t1);
                    list.Add(t2);
                }
            }

            return list;
        }
    }
     * 
     * 
     */
}

/*
使用方法:
在Script中或Scenario或Strategy中使用
CTPAPI.GetInstance().SpreadMarketData = new MySpreadMarketData();

注意:重复设置会覆盖
 */

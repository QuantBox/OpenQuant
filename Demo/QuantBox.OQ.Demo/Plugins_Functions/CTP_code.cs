using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;

using QuantBox.Helper.CTP;
using QuantBox.CSharp2CTP;

namespace QuantBox.OQ.Demo.Plugins_Functions
{
    /// <summary>
    /// 取合约、行情、保证金率与手续费
    /// 订阅深度行情、交易所状态
    /// 
    /// OnRspQryDepthMarketData:查一次，返回一次。通过交易接口。没有订阅的可以返回
    /// OnRtnDepthMarketData：是总是返回。通过行情接口。只能返回订阅过的。
    /// 这个事件在最后触发，优先级循序为：Trade\Quote\MarketDepth\OnRtnDepthMarketData
    /// 
    /// 要使用OnRtnDepthMarketData,请将插件属性中的EmitOnRtnDepthMarketData改成True
    /// </summary>
    public class CTP_code:Strategy
    {
        public override void OnStrategyStart()
        {
            CTPAPI.GetInstance().OnRspQryInstrument += new CTPAPI.RspQryInstrument(OnRspQryInstrument);
            CTPAPI.GetInstance().OnRspQryInstrumentMarginRate += new CTPAPI.RspQryInstrumentMarginRate(OnRspQryInstrumentMarginRate);
            CTPAPI.GetInstance().OnRspQryInstrumentCommissionRate += new CTPAPI.RspQryInstrumentCommissionRate(OnRspQryInstrumentCommissionRate);
            // 交易所状态
            CTPAPI.GetInstance().OnRtnInstrumentStatus += new CTPAPI.RtnInstrumentStatus(OnRtnInstrumentStatus);

            CTPAPI.GetInstance().OnRspQryTradingAccount += new CTPAPI.RspQryTradingAccount(OnRspQryTradingAccount);

            CTPAPI.GetInstance().OnRspReqQryInvestorPosition += new CTPAPI.RspReqQryInvestorPosition(OnRspReqQryInvestorPosition);

            // 此方法已经过期，在插件3.8.2.0中开始废弃
            //CTPAPI.GetInstance().OnRtnDepthMarketData += new CTPAPI.RtnDepthMarketData(OnRtnDepthMarketData);
        }

        void OnRspReqQryInvestorPosition(CThostFtdcInvestorPositionField pInvestorPosition)
        {
            Console.WriteLine("==持仓");
            Console.WriteLine(pInvestorPosition.InstrumentID);
        }

        void OnRspQryTradingAccount(CThostFtdcTradingAccountField pTradingAccount)
        {
            Console.WriteLine("==资金");
            Console.WriteLine(pTradingAccount.Balance);
            Console.WriteLine(pTradingAccount.TradingDay);
        }

        public override void OnStrategyStop()
        {
            CTPAPI.GetInstance().OnRspQryInstrument -= new CTPAPI.RspQryInstrument(OnRspQryInstrument);
            CTPAPI.GetInstance().OnRspQryInstrumentMarginRate -= new CTPAPI.RspQryInstrumentMarginRate(OnRspQryInstrumentMarginRate);
            CTPAPI.GetInstance().OnRspQryInstrumentCommissionRate -= new CTPAPI.RspQryInstrumentCommissionRate(OnRspQryInstrumentCommissionRate);
            
            CTPAPI.GetInstance().OnRtnInstrumentStatus -= new CTPAPI.RtnInstrumentStatus(OnRtnInstrumentStatus);

            CTPAPI.GetInstance().OnRspQryTradingAccount -= new CTPAPI.RspQryTradingAccount(OnRspQryTradingAccount);

            // 此方法已经过期，在插件3.8.2.0中开始废弃
            //CTPAPI.GetInstance().OnRtnDepthMarketData -= new CTPAPI.RtnDepthMarketData(OnRtnDepthMarketData);
        }

        public override void OnBar(Bar bar)
        {
            //以下四句只是演示，实盘中请保存查询出来的结果，并且一天只查一次
            CTPAPI.GetInstance().ReqQryInstrument("IF1312");

            // 一定得加上TThostFtdcHedgeFlagType
            CTPAPI.GetInstance().ReqQryInstrumentMarginRate("IF1312", TThostFtdcHedgeFlagType.Speculation);

            //目前模拟平台目前查询没有返回，实盘时返回的是产品的手续费，如查"IF1312"，返回的是"IF"
            CTPAPI.GetInstance().ReqQryInstrumentCommissionRate("IF1312");

            //通过交易接口查询，主要用来取涨跌停或没有直接订阅的行情
            CTPAPI.GetInstance().ReqQryDepthMarketData("IF1309");
        }

        void OnRspQryInstrumentMarginRate(CThostFtdcInstrumentMarginRateField pInstrumentMarginRate)
        {
            Console.WriteLine("==保证金率");
            Console.WriteLine(pInstrumentMarginRate.InstrumentID);
            Console.WriteLine(pInstrumentMarginRate.LongMarginRatioByMoney);
            Console.WriteLine(pInstrumentMarginRate.LongMarginRatioByVolume);
            Console.WriteLine(pInstrumentMarginRate.ShortMarginRatioByMoney);
            Console.WriteLine(pInstrumentMarginRate.ShortMarginRatioByVolume);
        }

        void OnRspQryInstrument(CThostFtdcInstrumentField pInstrument)
        {
            Console.WriteLine("==合约列表");
            Console.WriteLine(pInstrument.LongMarginRatio);
            Console.WriteLine(pInstrument.ShortMarginRatio);
        }

        void OnRspQryInstrumentCommissionRate(CThostFtdcInstrumentCommissionRateField pInstrumentCommissionRate)
        {
            Console.WriteLine("==手续费率");
            Console.WriteLine(pInstrumentCommissionRate.CloseRatioByMoney);
            Console.WriteLine(pInstrumentCommissionRate.CloseRatioByVolume);
        }

        void OnRtnInstrumentStatus(CThostFtdcInstrumentStatusField pInstrumentStatus)
        {
            // 由于只在有新消息过来时才会收到，所以可能认为不好测试，看不出效果
            // 在测试时，把插件属性的ResumeType改成THOST_TERT_RESTART，然后连接CTP就可以看到效果了。
            Console.WriteLine("==交易所状态");
            Console.WriteLine("{0},{1},{2}",
                    pInstrumentStatus.ExchangeID, pInstrumentStatus.InstrumentID,
                    pInstrumentStatus.InstrumentStatus);
        }

        //void OnRspQryDepthMarketData(CThostFtdcDepthMarketDataField pDepthMarketData)
        //{
        //    Console.WriteLine("==取深度行情");
        //    Console.WriteLine(pDepthMarketData.InstrumentID);
        //    Console.WriteLine(pDepthMarketData.LastPrice);
        //    Console.WriteLine(pDepthMarketData.UpperLimitPrice);
        //    Console.WriteLine(pDepthMarketData.LowerLimitPrice);
        //}

        void OnRtnDepthMarketData(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            // 得打开EmitOnRtnDepthMarketData开关
            Console.WriteLine("++订阅深度行情");
            Console.WriteLine(pDepthMarketData.InstrumentID);
            Console.WriteLine(pDepthMarketData.LastPrice);
            Console.WriteLine(pDepthMarketData.OpenInterest);
        }
    }
}

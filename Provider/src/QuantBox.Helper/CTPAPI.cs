using System;
using System.Collections;
using QuantBox.CSharp2C;
using System.Collections.Generic;

namespace QuantBox.Helper
{
    public sealed class CTPAPI
    {
        private static readonly CTPAPI instance = new CTPAPI();
        private CTPAPI()
        {
        }
        public static CTPAPI GetInstance()
        {
            return instance;
        }


        private IntPtr m_pMdApi = IntPtr.Zero;      //行情对象指针
        private IntPtr m_pTdApi = IntPtr.Zero;      //交易对象指针

        public void __RegTdApi(IntPtr pTdApi)
        {
            m_pTdApi = pTdApi;
        }

        public void __RegMdApi(IntPtr pMdApi)
        {
            m_pMdApi = pMdApi;
        }

        #region 合列列表
        private Dictionary<string, CThostFtdcInstrumentField> _dictInstruments = null;
        public void __RegInstrumentDictionary(Dictionary<string, CThostFtdcInstrumentField> dict)
        {
            _dictInstruments = dict;
        }

        public delegate void RspQryInstrument(CThostFtdcInstrumentField pInstrument);
        public event RspQryInstrument OnRspQryInstrument;
        public void FireOnRspQryInstrument(CThostFtdcInstrumentField pInstrument)
        {
            if (null != OnRspQryInstrument)
            {
                OnRspQryInstrument(pInstrument);
            }
        }

        public void ReqQryInstrument(string instrument)
        {
            if (null != _dictInstruments)
            {
                CThostFtdcInstrumentField value;
                if (_dictInstruments.TryGetValue(instrument, out value))
                {
                    FireOnRspQryInstrument(value);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(instrument)
                    && null != m_pTdApi
                    && IntPtr.Zero != m_pTdApi)
            {
                TraderApi.TD_ReqQryInstrument(m_pTdApi, instrument);
            }
        }
        #endregion

        #region 保证金率
        private Dictionary<string, CThostFtdcInstrumentMarginRateField> _dictMarginRate = null;
        public void __RegInstrumentMarginRateDictionary(Dictionary<string, CThostFtdcInstrumentMarginRateField> dict)
        {
            _dictMarginRate = dict;
        }
        public void ReqQryInstrumentMarginRate(string instrument)
        {
            if (null != _dictMarginRate)
            {
                CThostFtdcInstrumentMarginRateField value;
                if (_dictMarginRate.TryGetValue(instrument, out value))
                {
                    FireOnRspQryInstrumentMarginRate(value);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(instrument)
                && null != m_pTdApi
                && IntPtr.Zero != m_pTdApi)
            {
                TraderApi.TD_ReqQryInstrumentMarginRate(m_pTdApi, instrument);
            }
        }        

        public delegate void RspQryInstrumentMarginRate(CThostFtdcInstrumentMarginRateField pInstrumentMarginRate);
        public event RspQryInstrumentMarginRate OnRspQryInstrumentMarginRate;
        public void FireOnRspQryInstrumentMarginRate(CThostFtdcInstrumentMarginRateField pInstrumentMarginRate)
        {
            if (null != OnRspQryInstrumentMarginRate)
            {
                OnRspQryInstrumentMarginRate(pInstrumentMarginRate);
            }
        }
        #endregion

        #region 手续费率
        private Dictionary<string, CThostFtdcInstrumentCommissionRateField> _dictCommissionRate = null;
        public void __RegInstrumentCommissionRateDictionary(Dictionary<string, CThostFtdcInstrumentCommissionRateField> dict)
        {
            _dictCommissionRate = dict;
        }

        public void ReqQryInstrumentCommissionRate(string instrument)
        {
            if (null != _dictCommissionRate)
            {
                CThostFtdcInstrumentCommissionRateField value;
                if (_dictCommissionRate.TryGetValue(instrument, out value))
                {
                    FireOnRspQryInstrumentCommissionRate(value);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(instrument)
                && null != m_pTdApi
                && IntPtr.Zero != m_pTdApi)
            {
                TraderApi.TD_ReqQryInstrumentCommissionRate(m_pTdApi, instrument);
            }
        }

        public delegate void RspQryInstrumentCommissionRate(CThostFtdcInstrumentCommissionRateField pInstrumentCommissionRate);
        public event RspQryInstrumentCommissionRate OnRspQryInstrumentCommissionRate;
        public void FireOnRspQryInstrumentCommissionRate(CThostFtdcInstrumentCommissionRateField pInstrumentCommissionRate)
        {
            if (null != OnRspQryInstrumentCommissionRate)
            {
                OnRspQryInstrumentCommissionRate(pInstrumentCommissionRate);
            }
        }
        #endregion

        #region 深度行情
        private Dictionary<string, CThostFtdcDepthMarketDataField> _dictDepthMarketData = null;
        public void __RegDepthMarketDataDictionary(Dictionary<string, CThostFtdcDepthMarketDataField> dict)
        {
            _dictDepthMarketData = dict;
        }

        public void ReqQryDepthMarketData(string instrument)
        {
            if (null != _dictDepthMarketData)
            {
                CThostFtdcDepthMarketDataField value;
                if (_dictDepthMarketData.TryGetValue(instrument, out value))
                {
                    FireOnRspQryDepthMarketData(value);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(instrument)
                && null != m_pTdApi
                && IntPtr.Zero != m_pTdApi)
            {
                TraderApi.TD_ReqQryDepthMarketData(m_pTdApi, instrument);
            }
        }

        public delegate void RspQryDepthMarketData(CThostFtdcDepthMarketDataField pDepthMarketData);
        public event RspQryDepthMarketData OnRspQryDepthMarketData;
        public void FireOnRspQryDepthMarketData(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            if (null != OnRspQryDepthMarketData)
            {
                OnRspQryDepthMarketData(pDepthMarketData);
            }
        }
        #endregion
    }
}

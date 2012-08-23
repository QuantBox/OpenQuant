using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartQuant.Providers;
using SmartQuant.FIX;
using SmartQuant.Instruments;
using QuantBox.CSharp2C;

namespace QuantBox.OQ.CTP
{
    public partial class QBProvider : IInstrumentProvider
    {
        public event SecurityDefinitionEventHandler SecurityDefinition;

        public void SendSecurityDefinitionRequest(FIXSecurityDefinitionRequest request)
        {
            lock (this)
            {
                if (!_bTdConnected)
                {
                    this.EmitError(-1,-1,"交易没有连接，无法获取合约列表");
                    return;
                }

                string symbol = request.ContainsField(EFIXField.Symbol) ? request.Symbol : null;
                string securityType = request.ContainsField(EFIXField.SecurityType) ? request.SecurityType : null;
                string securityExchange = request.ContainsField(EFIXField.SecurityExchange) ? request.SecurityExchange : null;

                #region 过滤
                List<CThostFtdcInstrumentField> list = new List<CThostFtdcInstrumentField>();
                foreach (CThostFtdcInstrumentField inst in _dictInstruments.Values)
                {
                    int flag = 0;
                    if (null == symbol)
                    {
                        ++flag;
                    }
                    else if (inst.InstrumentID.ToUpper().StartsWith(symbol.ToUpper()))
                    {
                        ++flag;
                    }

                    if (null == securityExchange)
                    {
                        ++flag;
                    }
                    else if (inst.ExchangeID.ToUpper().StartsWith(securityExchange.ToUpper()))
                    {
                        ++flag;
                    }

                    if (null == securityType)
                    {
                        ++flag;
                    }
                    else
                    {
                        if (FIXSecurityType.Future == securityType)
                        {
                            if (TThostFtdcProductClassType.Futures == inst.ProductClass)
                            {
                                ++flag;
                            }
                        }
                        else if (FIXSecurityType.MultiLegInstrument == securityType)//理解上是否有问题
                        {
                            if (TThostFtdcProductClassType.Combination == inst.ProductClass)
                            {
                                ++flag;
                            }
                        }
                        else if (FIXSecurityType.Option == securityType)
                        {
                            if (TThostFtdcProductClassType.Options == inst.ProductClass)
                            {
                                ++flag;
                            }
                        }
                    }
                    
                    if (3==flag)
                    {
                        list.Add(inst);
                    }
                }
                #endregion

                list.Sort(SortCThostFtdcInstrumentField);

                //如果查出的数据为0，应当想法立即返回
                if (0==list.Count)
                {
                    FIXSecurityDefinition definition = new FIXSecurityDefinition
                    {
                        SecurityReqID = request.SecurityReqID,
                        SecurityResponseID = request.SecurityReqID,
                        SecurityResponseType = request.SecurityRequestType,
                        TotNoRelatedSym = 1//有个除0错误的问题
                    };
                    if (SecurityDefinition != null)
                    {
                        SecurityDefinition(this, new SecurityDefinitionEventArgs(definition));
                    }
                }

                foreach (CThostFtdcInstrumentField inst in list)
                {
                    FIXSecurityDefinition definition = new FIXSecurityDefinition
                    {
                        SecurityReqID = request.SecurityReqID,
                        SecurityResponseID = request.SecurityReqID,
                        SecurityResponseType = request.SecurityRequestType,
                        TotNoRelatedSym = list.Count
                    };

                    {
                        string securityType2;
                        switch (inst.ProductClass)
                        {
                            case TThostFtdcProductClassType.Futures:
                                securityType2 = FIXSecurityType.Future;
                                break;
                            case TThostFtdcProductClassType.Combination:
                                securityType2 = FIXSecurityType.MultiLegInstrument;//此处是否理解上有不同
                                break;
                            case TThostFtdcProductClassType.Options:
                                securityType2 = FIXSecurityType.Option;
                                break;
                            default:
                                securityType2 = FIXSecurityType.NoSecurityType;
                                break;
                        }
                        definition.AddField(EFIXField.SecurityType, securityType2);
                    }
                    {
                        double x = inst.PriceTick;
                        int i = 0;
                        for (; x - (int)x != 0; ++i)
                        {
                            x = x * 10;
                        }
                        definition.AddField(EFIXField.PriceDisplay, string.Format("F{0}",i));
                    }

                    definition.AddField(EFIXField.Symbol, inst.InstrumentID);
                    definition.AddField(EFIXField.SecurityExchange, inst.ExchangeID);
                    definition.AddField(EFIXField.Currency, "CNY");//Currency.CNY
                    definition.AddField(EFIXField.TickSize, inst.PriceTick);
                    definition.AddField(EFIXField.SecurityDesc, inst.InstrumentName);
                    definition.AddField(EFIXField.MaturityDate, DateTime.ParseExact(inst.ExpireDate, "yyyyMMdd", null));
                    definition.AddField(EFIXField.Factor, (double)inst.VolumeMultiple);
                    //还得补全内容

                    if (SecurityDefinition != null)
                    {
                        SecurityDefinition(this, new SecurityDefinitionEventArgs(definition));
                    }
                }
            }
        }

        private static int SortCThostFtdcInstrumentField(CThostFtdcInstrumentField a1, CThostFtdcInstrumentField a2)
        {
            return a1.InstrumentID.CompareTo(a2.InstrumentID);
        }
    }
}

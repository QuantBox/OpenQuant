using System;
using System.Collections.Generic;
using System.Data;
using QuantBox.CSharp2C;
using SmartQuant;
using SmartQuant.Execution;
using SmartQuant.FIX;
using SmartQuant.Providers;

namespace QuantBox.OQ.CTP
{
    public partial class QBProvider : IExecutionProvider
    {
        private Dictionary<SingleOrder, OrderRecord> orderRecords = new Dictionary<SingleOrder, OrderRecord>();

        public event ExecutionReportEventHandler ExecutionReport;
        public event OrderCancelRejectEventHandler OrderCancelReject;

        public BrokerInfo GetBrokerInfo()
        {
            BrokerInfo brokerInfo = new BrokerInfo();

            if (IsConnected)
            {
                Console.WriteLine(string.Format("GetBrokerInfo:{0}", Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                //TraderApi.TD_ReqQryTradingAccount(m_pTdApi);
                //TraderApi.TD_ReqQryInvestorPosition(m_pTdApi, null);
                //timerAccount.Enabled = false;
                //timerAccount.Enabled = true;
                //timerPonstion.Enabled = false;
                //timerPonstion.Enabled = true;    

                BrokerAccount brokerAccount = new BrokerAccount(m_TradingAccount.AccountID);

                // account fields
                brokerAccount.BuyingPower = m_TradingAccount.Available;

                brokerAccount.AddField("Available", m_TradingAccount.Available.ToString());
                brokerAccount.AddField("Balance", m_TradingAccount.Balance.ToString());
                brokerAccount.AddField("CashIn", m_TradingAccount.CashIn.ToString());
                brokerAccount.AddField("CloseProfit", m_TradingAccount.CloseProfit.ToString());
                brokerAccount.AddField("Commission", m_TradingAccount.Commission.ToString());
                brokerAccount.AddField("Credit", m_TradingAccount.Credit.ToString());
                brokerAccount.AddField("CurrMargin", m_TradingAccount.CurrMargin.ToString());
                brokerAccount.AddField("DeliveryMargin", m_TradingAccount.DeliveryMargin.ToString());
                brokerAccount.AddField("Deposit", m_TradingAccount.Deposit.ToString());
                brokerAccount.AddField("ExchangeDeliveryMargin", m_TradingAccount.ExchangeDeliveryMargin.ToString());
                brokerAccount.AddField("ExchangeMargin", m_TradingAccount.ExchangeMargin.ToString());
                brokerAccount.AddField("FrozenCash", m_TradingAccount.FrozenCash.ToString());
                brokerAccount.AddField("FrozenCommission", m_TradingAccount.FrozenCommission.ToString());
                brokerAccount.AddField("FrozenMargin", m_TradingAccount.FrozenMargin.ToString());
                brokerAccount.AddField("Interest", m_TradingAccount.Interest.ToString());
                brokerAccount.AddField("InterestBase", m_TradingAccount.InterestBase.ToString());
                brokerAccount.AddField("Mortgage", m_TradingAccount.Mortgage.ToString());
                brokerAccount.AddField("PositionProfit", m_TradingAccount.PositionProfit.ToString());
                brokerAccount.AddField("PreBalance", m_TradingAccount.PreBalance.ToString());
                brokerAccount.AddField("PreCredit", m_TradingAccount.PreCredit.ToString());
                brokerAccount.AddField("PreDeposit", m_TradingAccount.PreDeposit.ToString());
                brokerAccount.AddField("PreMargin", m_TradingAccount.PreMargin.ToString());
                brokerAccount.AddField("PreMortgage", m_TradingAccount.PreMortgage.ToString());
                brokerAccount.AddField("Reserve", m_TradingAccount.Reserve.ToString());
                brokerAccount.AddField("SettlementID", m_TradingAccount.SettlementID.ToString());
                brokerAccount.AddField("Withdraw", m_TradingAccount.Withdraw.ToString());
                brokerAccount.AddField("WithdrawQuota", m_TradingAccount.WithdrawQuota.ToString());

                DataRow[] rows = _dbInMemInvestorPosition.SelectAll();

                foreach (DataRow dr in rows)
                {
                    BrokerPosition brokerPosition = new BrokerPosition {
                        Symbol = dr[DbInMemInvestorPosition.InstrumentID].ToString()
                    };

                    int pos = (int)dr[DbInMemInvestorPosition.Position];
                    TThostFtdcPosiDirectionType PosiDirection = (TThostFtdcPosiDirectionType)dr[DbInMemInvestorPosition.PosiDirection];
                    if (TThostFtdcPosiDirectionType.Long == PosiDirection)
                    {
                        brokerPosition.LongQty = pos;
                    }
                    else if (TThostFtdcPosiDirectionType.Short == PosiDirection)
                    {
                        brokerPosition.ShortQty = pos;
                    }
                    else
                    {
                        if (pos >= 0)//净NET这个概念是什么情况？
                            brokerPosition.LongQty = pos;
                        else
                            brokerPosition.ShortQty = -pos;
                    }
                    brokerPosition.Qty = brokerPosition.LongQty - brokerPosition.ShortQty;
                    brokerPosition.AddCustomField(DbInMemInvestorPosition.PosiDirection, PosiDirection.ToString());
                    brokerPosition.AddCustomField(DbInMemInvestorPosition.HedgeFlag, ((TThostFtdcHedgeFlagType)dr[DbInMemInvestorPosition.HedgeFlag]).ToString());
                    brokerPosition.AddCustomField(DbInMemInvestorPosition.PositionDate, ((TThostFtdcPositionDateType)dr[DbInMemInvestorPosition.PositionDate]).ToString());
                    brokerAccount.AddPosition(brokerPosition);
                }
                brokerInfo.Accounts.Add(brokerAccount);
            }            

            return brokerInfo;
        }

        public void SendNewOrderSingle(NewOrderSingle order)
        {
            SingleOrder key = order as SingleOrder;
            this.orderRecords.Add(key, new OrderRecord(key));
            Send(key);
        }

        #region OpenQuant下的接口
        public void SendOrderCancelReplaceRequest(OrderCancelReplaceRequest request)
        {
            SendOrderCancelReplaceRequest(request as FIXOrderCancelReplaceRequest);
        }

        public void SendOrderCancelRequest(OrderCancelRequest request)
        {
            SendOrderCancelRequest(request as FIXOrderCancelRequest);
        }

        public void SendOrderStatusRequest(OrderStatusRequest request)
        {
            SendOrderStatusRequest(request as FIXOrderStatusRequest);
        }
        #endregion

        #region QuantDeveloper下的接口
        public void SendOrderCancelReplaceRequest(FIXOrderCancelReplaceRequest request)
        {
            //IOrder order = OrderManager.Orders.All[request.OrigClOrdID];
            //SingleOrder order2 = order as SingleOrder;
            //this.provider.CallReplace(order2);
            EmitError(-1,-1,"不支持改单指令");
        }

        public void SendOrderCancelRequest(FIXOrderCancelRequest request)
        {
            IOrder order = OrderManager.Orders.All[request.OrigClOrdID];
            SingleOrder order2 = order as SingleOrder;
            Cancel(order2);
        }

        public void SendOrderStatusRequest(FIXOrderStatusRequest request)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void EmitExecutionReport(ExecutionReport report)
        {
            if (ExecutionReport != null)
            {
                ExecutionReport(this, new ExecutionReportEventArgs(report));
            }
        }

        private void EmitOrderCancelReject()
        {
            if (OrderCancelReject != null)
            {
                OrderCancelReject(this, new OrderCancelRejectEventArgs(null));
            }
        }

        public void EmitExecutionReport(SingleOrder order, OrdStatus status)
        {
            EmitExecutionReport(order, status, "");
        }

        public void EmitExecutionReport(SingleOrder order, OrdStatus status, string text)
        {
            OrderRecord record = this.orderRecords[order];
            EmitExecutionReport(record, status, 0.0, 0, text);
        }

        public void EmitExecutionReport(SingleOrder order, double price, int quantity)
        {
            OrderRecord record = this.orderRecords[order];
            EmitExecutionReport(record, OrdStatus.Undefined, price, quantity, "");
        }

        private void EmitExecutionReport(OrderRecord record, OrdStatus ordStatus, double lastPx, int lastQty, string text)
        {
            ExecutionReport report = new ExecutionReport
            {
                TransactTime = Clock.Now,
                ClOrdID = record.Order.ClOrdID,
                OrigClOrdID = record.Order.ClOrdID,
                OrderID = record.Order.OrderID,
                Symbol = record.Order.Symbol,
                SecurityType = record.Order.SecurityType,
                SecurityExchange = record.Order.SecurityExchange,
                Currency = record.Order.Currency,
                Side = record.Order.Side,
                OrdType = record.Order.OrdType,
                TimeInForce = record.Order.TimeInForce,
                OrderQty = record.Order.OrderQty,
                Price = record.Order.Price,
                StopPx = record.Order.StopPx,
                LastPx = lastPx,
                LastQty = lastQty
            };
            if (ordStatus == OrdStatus.Undefined)
            {
                record.AddFill(lastPx, lastQty);
                if (record.LeavesQty > 0)
                {
                    ordStatus = OrdStatus.PartiallyFilled;
                }
                else
                {
                    ordStatus = OrdStatus.Filled;
                }
            }
            report.AvgPx = record.AvgPx;
            report.CumQty = record.CumQty;
            report.LeavesQty = record.LeavesQty;
            report.ExecType = this.GetExecType(ordStatus);
            report.OrdStatus = ordStatus;
            report.Text = text;

            EmitExecutionReport(report);
        }

        protected void EmitAccepted(SingleOrder order)
        {
            EmitExecutionReport(order, OrdStatus.New);
        }

        protected void EmitCancelled(SingleOrder order)
        {
            EmitExecutionReport(order, OrdStatus.Cancelled);
        }

        protected void EmitCancelReject(SingleOrder order, string message)
        {
            //EmitCancelReject(order, message);
        }

        protected void EmitFilled(SingleOrder order, double price, int quantity)
        {
            EmitExecutionReport(order, price, quantity);
        }

        protected void EmitRejected(SingleOrder order, string message)
        {
            EmitExecutionReport(order, OrdStatus.Rejected, message);
        }

        private ExecType GetExecType(OrdStatus status)
        {
            switch (status)
            {
                case OrdStatus.New:
                    return ExecType.New;
                case OrdStatus.PartiallyFilled:
                    return ExecType.PartialFill;
                case OrdStatus.Filled:
                    return ExecType.Fill;
                case OrdStatus.Cancelled:
                    return ExecType.Cancelled;
                case OrdStatus.Replaced:
                    return ExecType.Replace;
                case OrdStatus.PendingCancel:
                    return ExecType.PendingCancel;
                case OrdStatus.Rejected:
                    return ExecType.Rejected;
                case OrdStatus.PendingReplace:
                    return ExecType.PendingReplace;
            }
            throw new ArgumentException(string.Format("Cannot find exec type for ord status - {0}", status));
        }

        #region OpenQuant3接口中的新方法
        public void RegisterOrder(NewOrderSingle order)
        {
        }
        #endregion
    }
}

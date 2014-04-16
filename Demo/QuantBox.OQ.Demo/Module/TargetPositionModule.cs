using OpenQuant.API;
using QuantBox.OQ.Demo.Helper;
using QuantBox.OQ.Extensions;
using QuantBox.OQ.Extensions.OrderText;
using System;

namespace QuantBox.OQ.Demo.Module
{
    public class TargetPositionModule : Strategy
    {
        public DualPosition DualPosition;
        /// <summary>
        /// 目标仓位
        /// </summary>
        public double TargetPosition = 0;
        /// <summary>
        /// 每笔最大手数
        /// </summary>
        [Parameter("每笔最大手数", "TargetPositionModule")]
        public double MaxQtyPerLot = 5;
        /// <summary>
        /// 小于等于此数量的开仓单自动以市价发送
        /// </summary>
        [Parameter("小于等于此数量的开仓单自动以市价发送", "TargetPositionModule")]
        public double MarketOpenQtyThreshold = 5;
        /// <summary>
        /// 小于等于此数量的平仓单自动以市价发送
        /// </summary>
        [Parameter("小于等于此数量的平仓单自动以市价发送", "TargetPositionModule")]
        public double MarketCloseQtyThreshold = 20;
        /// <summary>
        /// 对手价上加N跳下单
        /// </summary>
        [Parameter("对手价上加N跳下单", "TargetPositionModule")]
        public int Jump = 2;

        protected TimeHelper TimeHelper;
        protected PriceHelper PriceHelper;
        protected CloseTodayHelper CloseTodayHelper;

        protected TextCommon TextParameter;

        /// <summary>
        /// 入场后的最高价，用于跟踪止损
        /// </summary>
        public double HighestAfterEntry = double.MinValue;
        /// <summary>
        /// 入场后的最低价，用于跟踪止损
        /// </summary>
        public double LowestAfterEntry = double.MaxValue;

        /// <summary>
        /// 在进行各项计算时到底是使用信号持仓还是实盘持仓呢？
        /// </summary>
        /// <returns></returns>
        public virtual double GetCurrentQty()
        {
            // 各项处理时使用实盘持仓
            return DualPosition.NetQty;
            // 各项处理时使用信号持仓
            //return TargetPosition;
        }

        public virtual double GetLongAvgPrice()
        {
            return DualPosition.Long.AvgPrice;
        }

        public virtual double GetShortAvgPrice()
        {
            return DualPosition.Short.AvgPrice;
        }

        /// <summary>
        /// 计算入场后的最高价与最低价
        /// </summary>
        /// <param name="price"></param>
        public void HiLoAfterEntry(double price)
        {
            double qty = GetCurrentQty();
            if (qty == 0)
            {
                HighestAfterEntry = double.MinValue;
                LowestAfterEntry = double.MaxValue;
            }
            else
            {
                HighestAfterEntry = Math.Max(HighestAfterEntry, price);
                LowestAfterEntry = Math.Min(LowestAfterEntry, price);
            }
        }

        public override void OnStrategyStart()
        {
            TimeHelper = new TimeHelper(Instrument.Symbol);
            PriceHelper = new PriceHelper(Instrument.TickSize);
            TextParameter = new TextCommon() { OpenClose = EnumOpenClose.OPEN };
            CloseTodayHelper = new CloseTodayHelper(EnumExchangeID.SHFE);

            DualPosition = new DualPosition(Instrument.Symbol);
            DualPosition.Sell.PriceHelper = PriceHelper;
            DualPosition.Buy.PriceHelper = PriceHelper;
            DualPosition.CloseTodayHelper = CloseTodayHelper;

            TargetPosition = 0;
            DualPosition.Long.Qty = 0;
            DualPosition.Long.QtyToday = 0;
            DualPosition.Short.Qty = 0;
            DualPosition.Short.QtyToday = 0;
        }

        public override void OnTrade(Trade trade)
        {
            lock(this)
            {
                Process();
                HiLoAfterEntry(trade.Price);
            }
        }

        public override void OnQuote(Quote quote)
        {
            lock(this)
            {
                Process();
            }

            // 如果只有Quote数据，如何更新？
            // 那就会不用这个目标仓位助手了
            //if(!DataRequests.HasTradeRequest)
            //{ 
            //}
        }

        public override void OnBarOpen(Bar bar)
        {
            lock(this)
            {
                Process();
                HiLoAfterEntry(bar.Open);
            }
        }

        public override void OnBar(Bar bar)
        {
            lock(this)
            {
                Process();
                HiLoAfterEntry(bar.Close);
            }
        }

        /// <summary>
        /// 进行下单，最小手续费处理原则
        /// </summary>
        public virtual void Process()
        {
            lock(this)
            {
                // 非交易时段，不处理
                if (!TimeHelper.IsTradingTime())
                {
                    return;
                }

                // 计算仓差
                double dif = TargetPosition - DualPosition.NetQty;
                double qty = 0;
                OrderSide Side = OrderSide.Buy;
                TextParameter.OpenClose = EnumOpenClose.OPEN;

                if (dif == 0)// 持仓量相等
                {
                    // 把所有的挂单全撤了
                    DualPosition.Cancel();
                    return;
                }
                else if (dif > 0 && !DualPosition.IsPending)// 表示要增加净持仓
                {
                    // 是否有在途增仓订单,超数了
                    // 是否有在途减仓订单,全取消息
                    qty = dif;
                    Side = OrderSide.Buy;

                    EnumOpenClose oc = EnumOpenClose.CLOSE;
                    double q = CloseTodayHelper.GetCloseAndQty(DualPosition.Short, out oc);
                    if (q > 0)
                    {
                        // 按最小数量进行平仓
                        qty = Math.Min(qty, q);
                        TextParameter.OpenClose = oc;
                    }
                }
                else if (!DualPosition.IsPending) // 减少净持仓
                {
                    qty = -dif;
                    Side = OrderSide.Sell;

                    EnumOpenClose oc = EnumOpenClose.CLOSE;
                    double q = CloseTodayHelper.GetCloseAndQty(DualPosition.Long, out oc);
                    if (q > 0)
                    {
                        // 按最小数量进行平仓
                        qty = Math.Min(qty, q);
                        TextParameter.OpenClose = oc;
                    }
                }

                if (qty > 0)
                {
                    qty = Math.Min(qty, MaxQtyPerLot);

                    // 下单
                    SendOrder(Side, qty);
                }
            }
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="side"></param>
        /// <param name="qty"></param>
        public void SendOrder(OrderSide side, double qty)
        {
            lock(this)
            {
                if (!TimeHelper.IsTradingTime())
                {
                    return;
                }

                if (qty <= 0)
                {
                    return;
                }

                // 为减少滑点，对数量少的单子直接市价单
                bool bMarketOrder = false;
                if (EnumOpenClose.OPEN == TextParameter.OpenClose)
                {
                    if (qty <= MarketOpenQtyThreshold)
                        bMarketOrder = true;
                }
                else
                {
                    if (qty <= MarketCloseQtyThreshold)
                        bMarketOrder = true;
                }

                if (bMarketOrder)
                {
                    SendMarketOrder(side, qty, TextParameter.ToString());
                }
                else
                {
                    SendLimitOrder(side, qty, PriceHelper.GetMatchPrice(this, side, Jump), TextParameter.ToString());
                }
            }
        }

        /// <summary>
        /// 重新发单
        /// </summary>
        /// <param name="order"></param>
        public void ResendOrder(Order order)
        {
            SendOrder(order.Side, order.LeavesQty);
        }

        public override void OnOrderPartiallyFilled(Order order)
        {
            lock(this)
            {
                DualPosition.Filled(order);
                HiLoAfterEntry(order.LastPrice);

                // 单子部分成交，不做操作，等单子完全执行完
                // 等交易完，会有问题，一直挂在上面不操作，新单子也过不来
                //Process();
            }
        }

        public override void OnOrderFilled(Order order)
        {
            lock(this)
            {
                DualPosition.Filled(order);
                HiLoAfterEntry(order.LastPrice);

                // 检查仓位是否正确,是否要发新单
                //Process();
            }
        }

        public override void OnNewOrder(Order order)
        {
            lock(this)
            {

                DualPosition.NewOrder(order);

                // 得加定时器，一定的时间内没有成交完全应当撤单重发，目前没有加这一功能
            }
        }

        public override void OnOrderRejected(Order order)
        {
            lock(this)
            {
                EnumOpenClose OpenClose = DualPosition.OrderRejected(order);

                double flag = order.Side == OrderSide.Buy ? 1 : -1;

                if (EnumOpenClose.OPEN == OpenClose)
                {
                    // 开仓被拒绝，不再新开仓
                    // 有可能是钱不够
                    // 有可能是超出持仓限制
                    // 有可能是非交易时间
                    TargetPosition -= flag * order.LeavesQty;
                    return;
                }

                EnumError error = TextResponse.FromText(order.Text);

                // 无法平仓，不重发单
                // 能出现这个问题是持仓计算错误，这已经是策略持仓计算错误了
                if (error == EnumError.OVER_CLOSETODAY_POSITION
                    || error == EnumError.OVER_CLOSEYESTERDAY_POSITION
                    || error == EnumError.OVER_CLOSE_POSITION)
                {
                    TargetPosition -= flag * order.LeavesQty;
                    return;
                }

                // 当前状态禁止此项操作,时间不对，应当等下次操作
            }
        }

        public override void OnOrderCancelled(Order order)
        {
            lock(this)
            {
                DualPosition.OrderCancelled(order);

                // 这个地方会影响做市商的挂单功能
                //ResendOrder(order);
            }
        }

        public override void OnOrderCancelReject(Order order)
        {
            // 撤单被拒绝，暂不操作
        }

        /// <summary>
        /// 根据入场后的最高与最低价，跟踪止损
        /// </summary>
        /// <param name="currentPrice"></param>
        /// <param name="level"></param>
        /// <param name="mode"></param>
        /// <returns>止损了返止损前持仓，可用于后面的反手</returns>
        public virtual double TrailingStop(double currentPrice, double level, StopMode mode,string text)
        {
            double qty = GetCurrentQty();
            double stop;
            if (qty > double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = HighestAfterEntry * (1.0 - level);
                }
                else
                {
                    stop = HighestAfterEntry - level;
                }
                if (currentPrice < stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("跟踪止损 - 最高{0},止损{1}>当前{2}|{3}",
                        HighestAfterEntry,stop,currentPrice,
                        text);
                    return qty;
                }
            }
            else if (qty < -double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = LowestAfterEntry * (1.0 + level);
                }
                else
                {
                    stop = LowestAfterEntry + level;
                }

                if (currentPrice > stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("跟踪止损 - 最低{0},止损{1}<当前{2}|{3}",
                        HighestAfterEntry, stop, currentPrice,
                        text);
                    return qty;
                }
            }

            return 0;
        }

        /// <summary>
        /// 从入场均价开始算起，固定点位止损
        /// </summary>
        /// <param name="currentPrice"></param>
        /// <param name="level"></param>
        /// <param name="mode"></param>
        /// <returns>止损了返止损前持仓，可用于后面的反手</returns>
        public virtual double FixedStop(double currentPrice, double level, StopMode mode, string text)
        {
            double qty = GetCurrentQty();
            double stop;
            if (qty > double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = GetLongAvgPrice() * (1.0 - level);
                }
                else
                {
                    stop = GetLongAvgPrice() - level;
                }
                if (currentPrice < stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("固定止损 - 多头均价{0},止损{1}>当前{2}|{3}",
                        GetLongAvgPrice(), stop, currentPrice,
                        text);
                    return qty;
                }
            }
            else if (qty < -double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = GetShortAvgPrice() * (1.0 + level);
                }
                else
                {
                    stop = GetShortAvgPrice() + level;
                }

                if (currentPrice > stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("固定止损 - 空头均价{0},止损{1}<当前{2}|{3}",
                        GetShortAvgPrice(), stop, currentPrice,
                        text);
                    return qty;
                }
            }
            return 0;
        }

        public virtual double TakeProfit(double currentPrice, double level, StopMode mode, string text)
        {
            double qty = GetCurrentQty();
            double stop;
            if (qty > double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = GetLongAvgPrice() * (1.0 + level);
                }
                else
                {
                    stop = GetLongAvgPrice() + level;
                }
                if (currentPrice > stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("固定止赢 - 多头均价{0},止赢{1}<当前{2}|{3}",
                        GetLongAvgPrice(), stop, currentPrice,
                        text);
                    return qty;
                }
            }
            else if (qty < -double.Epsilon)
            {
                if (StopMode.Percent == mode)
                {
                    stop = GetShortAvgPrice() * (1.0 - level);
                }
                else
                {
                    stop = GetShortAvgPrice() - level;
                }

                if (currentPrice < stop)
                {
                    TargetPosition = 0;
                    TextParameter.Text = string.Format("固定止赢 - 空头均价{0},止赢{1}>当前{2}|{3}",
                        GetShortAvgPrice(), stop, currentPrice,
                        text);
                    return qty;
                }
            }
            return 0;
        }

        /// <summary>
        /// 尾盘清仓
        /// </summary>
        /// <returns>返回上次持仓量</returns>
        public virtual double ExitOnClose(double seconds,string text)
        {
            double qty = GetCurrentQty();
            if (TimeHelper.GetTime(Clock.Now.AddSeconds(seconds)) >= TimeHelper.EndOfDay)
            {
                TargetPosition = 0;
                TextParameter.Text = string.Format("尾盘，清仓|{0}",text);
                return qty;
            }
            return 0;
        }

        public virtual void ChangeTradingDay()
        {
            if (TimeHelper.BeginOfDay > TimeHelper.EndOfDay)
            {
                // 夜盘
                if (TimeHelper.GetTime(Clock.Now) > TimeHelper.EndOfDay
                && TimeHelper.GetTime(Clock.Now) < TimeHelper.BeginOfDay)
                {
                    DualPosition.ChangeTradingDay();
                }
            }
            else
            {
                if (TimeHelper.GetTime(Clock.Now) > TimeHelper.EndOfDay
                || TimeHelper.GetTime(Clock.Now) < TimeHelper.BeginOfDay)
                {
                    DualPosition.ChangeTradingDay();
                }
            }
        }
    }

}

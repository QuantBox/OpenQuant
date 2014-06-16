﻿using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using QuantBox.OQ.Demo.Module;
using QuantBox.OQ.Demo.Helper;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// 双均线策略
    /// 
    /// 设置的是3分钟
    /// 
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=113
    /// </summary>
    public class DoubleMA_Crossover_code : TargetPositionModule
    {
        [OptimizationParameter(5, 10, 1)]
        [Parameter("快均线", "SMA")]
        int fastLength = 5;

        [OptimizationParameter(11, 15, 1)]
        [Parameter("慢均线", "SMA")]
        int slowLength = 12;

        [Parameter("交易手数")]
        double Qty = 1;

        [Parameter("时间周期，请按自己的设置修改")]
        int BarSize = 60;

        SMA fastSMA;
        SMA slowSMA;

        static BrokerInfo BrokerInfo;

        void LoadHistoricalBars(DateTime datetime)
        {
            DateTime dtEnd = datetime;
            DateTime dtBegin = dtEnd.AddDays(-5);//这个时间按自己的需求修改

            TradeSeries ts = DataManager.GetHistoricalTrades(Instrument, dtBegin, dtEnd);//获取历史交易数据
            //个人认为这个地方应当过滤下Trade数据，去除无效的再转换成Bars
            BarSeries bs = DataManager.CompressBars(ts, BarType.Time, BarSize);
            BarSeries barsMin = GetBars(BarType.Time, BarSize);
            foreach (Bar b in bs)
            {
                barsMin.Add(b);
            }
        }

        public override void OnStrategyStart()
        {
            base.OnStrategyStart();//开始基类策略

            // 测试用，自定义交易时间，仿真或实盘时可删除
            base.TimeHelper = new TimeHelper(new int[] { 0, 2400 },2100, 1458);

            base.TargetPosition = 0;
            //base.DualPosition.Long.Qty = 0;
            //base.DualPosition.Long.QtyToday = 0;
            //base.DualPosition.Short.Qty = 0;
            //base.DualPosition.Short.QtyToday = 0;
            if(BrokerInfo == null)
            {
                // 使用静态的主要原因是每个实例都来取一次没有必要
                BrokerInfo = DataManager.GetBrokerInfo();   
            }
            if (BrokerInfo.Accounts.Count > 0)
            {
                BrokerAccount brokerAccount = BrokerInfo.Accounts[0];//表示一组代理账户的属性,如名称、BuyingPower和位置列表。
                GetBrokerInfoHelper.Transform(brokerAccount, base.DualPosition);
            }
            

            LoadHistoricalBars(Clock.Now);

            BarSeries bars1min = GetBars(BarType.Time, BarSize);

            fastSMA = new SMA(bars1min, fastLength, Color.Red);
            slowSMA = new SMA(bars1min, slowLength, Color.Green);

            Draw(fastSMA, 0);
            Draw(slowSMA, 0);
        }
        /// <summary>
        /// OnBar事件
        /// </summary>
        /// <param name="bar"></param>
        public override void OnBar(Bar bar)
        {
            Cross cross = fastSMA.Crosses(slowSMA, bar);
            if (Cross.Above == cross)
            {
                base.TargetPosition = 1;
                TextParameter.Text = "金叉";
            }
            else if (Cross.Below == cross)
            {
                base.TargetPosition = -1;
                TextParameter.Text = "死叉";
            }
            else
            {
                // 保持上次的状态
            }

            base.ChangeTradingDay();

            base.OnBar(bar);
        }

        public override void OnTrade(Trade trade)
        {
            do
            {
                // 尾盘平仓
                if (0 != ExitOnClose(90,""))
                    break;

                // 跟踪止损
                TrailingStop(trade.Price, 5, StopMode.Absolute, "");

            } while (false);

            base.OnTrade(trade);
        }
    }

}

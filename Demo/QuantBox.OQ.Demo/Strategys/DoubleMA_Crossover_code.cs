using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using QuantBox.OQ.Demo.Module;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// 双均线策略
    /// 
    /// 设置的是3分钟
    /// 
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=113
    /// </summary>
    public class DoubleMA_Crossover_code : Strategy
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
        int BarSize = 180;

        SMA fastSMA;
        SMA slowSMA;

        bool isEnabled(DateTime datetime)
        {
            int nDatetime = datetime.Hour * 100 + datetime.Minute;
            if (nDatetime > 900 && nDatetime < 1459)
            {
                return true;
            }
            return false;
        }

        void LoadHistoricalBars(DateTime datetime)
        {
            DateTime dtEnd = datetime;
            DateTime dtBegin = dtEnd.AddDays(-5);//这个时间按自己的需求修改

            TradeSeries ts = DataManager.GetHistoricalTrades(Instrument, dtBegin, dtEnd);
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
            LoadHistoricalBars(Clock.Now);

            BarSeries bars1min = GetBars(BarType.Time, BarSize);

            fastSMA = new SMA(bars1min, fastLength, Color.Red);
            slowSMA = new SMA(bars1min, slowLength, Color.Green);

            Draw(fastSMA, 0);
            Draw(slowSMA, 0);
        }

        public override void OnBar(Bar bar)
        {
            Cross cross = fastSMA.Crosses(slowSMA, bar);
            if (Cross.Above == cross)
            {
                ClosePosition("T|买平，等反手");
                Buy(Qty, "O|买开");
            }
            else if (Cross.Below == cross)
            {
                ClosePosition("T|卖平，等反手");
                Sell(Qty, "O|卖开");

            }
            else if (HasPosition)
            {
                //可以在这写些加仓指令
            }
            return;
        }

        public override void OnPositionOpened()
        {
            //尾盘平仓
            DateTime dt = Clock.Now;
            SetStop(new DateTime(dt.Year, dt.Month, dt.Day, 14, 59, 0));

            //跟踪止损，10个价位
            SetStop(10, StopType.Trailing, StopMode.Absolute);
        }

        public override void OnStopExecuted(Stop stop)
        {
            ClosePosition("T|止损平仓");
        }
    }

}

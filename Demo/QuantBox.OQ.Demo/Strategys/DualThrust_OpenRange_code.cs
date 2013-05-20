using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

using QuantBox.OQ.Demo.Indicator;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// Dual Thrust & Open Range Break
    /// 
    /// 添加日线，添加1分钟线
    /// 在属性面板中设置策略类型，是DualThrust还是OpenRangeBreak.
    /// 当设置成OpenRangeBreak时，N设置无效
    /// 
    /// 没有历史数据加载的部分，请参考论坛上提到的从模拟切换到实盘的方法
    /// 
    /// 网上对这两个策略的配图
    /// http://gzqh.cnfol.com/111101/175,1787,11039812,00.shtml
    /// 论坛上DualThrust的TB版源码
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=195
    /// 论坛上OpenRangeBreak中文描述
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=398
    /// </summary>
    public class DualThrust_OpenRange_code : Strategy
    {
        public enum StrategyType
        {
            DualThrust,
            OpenRangeBreak,
        }

        [Parameter]
        StrategyType strategyType = StrategyType.DualThrust;
        [OptimizationParameter(0.4, 0.9, 0.1)]
        [Parameter("K1，上轨的参数")]
        double K1 = 0.5;
        [OptimizationParameter(0.4, 0.9, 0.1)]
        [Parameter("K2，下轨的参数")]
        double K2 = 0.5;
        [Parameter]
        double Range = 10;
        [Parameter]
        int N = 5;
        [Parameter]
        double Qty = 1;

        [Parameter("开盘时间，格式：hhmmss")]
        int openTime = 90000;
        [Parameter("尾盘开始清仓的时间，格式：hhmmss")]
        int closeTime = 145900;


        TimeSeries UpSeries;
        TimeSeries DownSeries;
        TimeSeries RangeSeries;

        PC HH;
        PC HC;
        PC LC;
        PC LL;

        double UpLine = double.NaN;
        double DownLine = double.NaN;

        BarSeries bars86400;

        public override void OnStrategyStart()
        {
            UpSeries = new TimeSeries("Up");
            DownSeries = new TimeSeries("Down");
            RangeSeries = new TimeSeries("Range");

            int n = N;
            if (StrategyType.OpenRangeBreak == strategyType)
            {
                n = 1;
            }

            bars86400 = GetBars(BarType.Time, 86400);
            HH = new PC(bars86400, n, BarData.High, PC.CalcType.Max, PC.UseLast.Yes);
            HC = new PC(bars86400, n, BarData.Close, PC.CalcType.Max, PC.UseLast.Yes);
            LC = new PC(bars86400, n, BarData.Close, PC.CalcType.Min, PC.UseLast.Yes);
            LL = new PC(bars86400, n, BarData.Low, PC.CalcType.Min, PC.UseLast.Yes);


            Draw(UpSeries, 0);
            Draw(DownSeries, 0);
            Draw(RangeSeries, 2);
        }

        public override void OnBarOpen(Bar bar)
        {
            if (86400 == bar.Size)
            {
                if (HH.Count < 1)
                    return;

                if (StrategyType.OpenRangeBreak == strategyType)
                {
                    Range = HH.Last - LL.Last;
                }
                else
                {
                    Range = Math.Max(HH.Last - LC.Last, HC.Last - LL.Last);
                }

                double dbOpen = bar.Open;
                UpLine = dbOpen + K1 * Range;
                DownLine = dbOpen - K2 * Range;
            }
        }

        public override void OnBar(Bar bar)
        {
            DateTime time = Clock.Now;
            int dateTime = time.Hour * 10000 + time.Minute * 100 + time.Second;

            if (dateTime > closeTime)
            {
                ClosePosition("T|尾盘平仓");
                return;
            }

            if (86400 == bar.Size)
            {
                return;
            }

            if (double.IsNaN(UpLine))
                return;

            UpSeries.Add(bar.DateTime, UpLine);
            DownSeries.Add(bar.DateTime, DownLine);
            RangeSeries.Add(bar.DateTime, Range);

            if (Mode == StrategyMode.Simulation)
            {
                // 从模拟切换实盘时用
                //return;
            }

            if (HasPosition)
            {
                if (Position.Amount < 0
                  && bar.Close > UpLine)
                {
                    ClosePosition("T|反手");
                    Buy(Qty, "O|");
                }
                if (Position.Amount > 0
                  && bar.Close < DownLine)
                {
                    ClosePosition("T|反手");
                    Sell(Qty, "O|");
                }
            }
            else
            {
                if (bar.Close > UpLine)
                {
                    Buy(Qty, "O|");
                }
                else if (bar.Close < DownLine)
                {
                    Sell(Qty, "O|");
                }
            }
        }
    }
}

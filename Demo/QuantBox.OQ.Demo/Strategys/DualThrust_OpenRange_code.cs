using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

using QuantBox.OQ.Demo.Indicator;
using QuantBox.OQ.Demo.Module;
using QuantBox.OQ.Demo.Helper;

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
    public class DualThrust_OpenRange_code : TargetPositionModule
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
            base.OnStrategyStart();

            // 测试用，自定义交易时间，仿真或实盘时可删除
            base.TimeHelper = new TimeHelper(new int[] { 0, 2400 },2100, 1458);

            base.TargetPosition = 0;
            base.DualPosition.Long.Qty = 0;
            base.DualPosition.Short.Qty = 0;

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
            do
            {
                if (86400 == bar.Size)
                {
                    if (HH.Count < 1)
                        break;

                    if (StrategyType.OpenRangeBreak == strategyType)
                    {
                        Range = HH.Last - LL.Last;
                    }
                    else
                    {
                        Range = Math.Max(HH.Last - LC.Last, HC.Last - LL.Last);
                    }

                    double dbOpen = bar.Open;
                    // 如果昨天波动过小，调整一下最小range
                    Range = Math.Max(Range, dbOpen * 0.01 * 0.2);

                    UpLine = dbOpen + K1 * Range;
                    DownLine = dbOpen - K2 * Range;

                    base.ChangeTradingDay();
                }
            } while (false);

            base.OnBarOpen(bar);
        }

        public override void OnBar(Bar bar)
        {
            do
            {
                // 尾盘平仓
                if (0 != ExitOnClose(60,""))
                    break;

                // 日线数据上不处理
                if (86400 == bar.Size)
                    break;

                // 数据
                if (double.IsNaN(UpLine))
                    break;

                UpSeries.Add(bar.DateTime, UpLine);
                DownSeries.Add(bar.DateTime, DownLine);
                RangeSeries.Add(bar.DateTime, Range);

                if (bar.Close > UpLine)
                {
                    TargetPosition = 1;
                    TextParameter.Text = "突破上轨，多头";
                }
                else if (bar.Close < DownLine)
                {
                    TargetPosition = -1;
                    TextParameter.Text = "突破下轨，空头";
                }
                else
                {
                    // 处于中间状态的怎么处理？
                }
            }
            while (false);

            if (Mode == StrategyMode.Simulation)
            {
                // 从模拟切换实盘时用
                //return;
            }

            base.OnBar(bar);
        }


        public override void OnTrade(Trade trade)
        {
            do
            {
                // 尾盘平仓
                if (0 != ExitOnClose(300,""))
                    break;

                // 跟踪止损
                TrailingStop(trade.Price, 5, StopMode.Absolute, "");

            } while (false);
            

            base.OnTrade(trade);
        }
    }
}

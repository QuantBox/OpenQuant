using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// 配对交易示例
    /// 主要靠静态变量来实现引用两个合约的数据，并计算价差。
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=345
    /// 
    /// 现在已经不推荐使用此方法，请参考MySpreadMarketData.cs
    /// </summary>
    public class PairTrading_code:Strategy
    {
        Instrument Instrument1 = InstrumentManager.Instruments["cu000"];
        Instrument Instrument2 = InstrumentManager.Instruments["zn000"];

        //static PairTrading_code Strategy1;
        //static PairTrading_code Strategy2;
        static BarSeries BarSeries1;
        static BarSeries BarSeries2;

        static TimeSeries spreadSeries = new TimeSeries("spread");

        [Parameter("两合约价格序列回归方程的系数")]
        double Slope = 4.325347;

        [Parameter("两合约价格序列回归方程的常数项")]
        double Const = -8358.336;

        [Parameter("时间窗口")]
        int Length = 100;

        int barSize = 60;

        SMA sma;

        public override void OnStrategyStart()
        {
            if (Instrument1 == Instrument)
            {
                //Strategy1 = this;
                BarSeries1 = GetBars(BarType.Time, barSize);
            }
            else if (Instrument2 == Instrument)
            {
                //Strategy2 = this;
                BarSeries2 = GetBars(BarType.Time, barSize);
            }
            else
            {
                Console.WriteLine("合约错误！" + Instrument);
            }

            sma = new SMA(spreadSeries, Length);

            Draw(spreadSeries, 2);
            Draw(sma, 2);
        }

        double Calculate(Bar bar1, Bar bar2)
        {
            return bar1.Close - bar2.Close * Slope - Const;
        }

        public override void OnBarSlice(long size)
        {
            if (Instrument1 == Instrument)
                return;

            if (BarSeries1.Count < 1 || BarSeries2.Count < 1)
                return;

            DateTime dt = BarSeries1.Last.DateTime;
            if (dt.CompareTo(BarSeries2.Last.DateTime) < 0)
            {
                dt = BarSeries2.Last.DateTime;
            }
            spreadSeries.Add(dt, Calculate(BarSeries1.Last, BarSeries2.Last));

            if (sma.Count < 1)
                return;

            //操作
            //Sell(Instrument2,Qty2,"O|卖开");
            //Buy(Instrument1,Qty1,"O|买开");
        }
    }
}

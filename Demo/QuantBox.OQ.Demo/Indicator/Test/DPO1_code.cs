using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator.Test
{
    public class DPO1_code : Strategy
    {
        DPO1 dpo;
        SMA sma;

        int length = 14;

        public override void OnStrategyStart()
        {
            dpo = new DPO1(Bars, length, BarData.Close);
            sma = new SMA(Bars, length, BarData.Close);

            Draw(dpo, 2);
            Draw(sma, 0);
        }

        public override void OnBar(Bar bar)
        {
            if (dpo.Count < 1)
                return;
            if (sma.Count < length)
                return;

            double d1 = bar.Close - sma.Ago(length / 2);
            double d2 = bar.Close - sma.Ago(length / 2 + 1);
            Console.WriteLine("{0},DPO:{1},{2},{3}",
               bar.DateTime, dpo.Last, d1, d2);
        }
    }
}

/*
6/4/2012 12:00:00 AM,DPO:-115.36,-115.36,-124.08
6/5/2012 12:00:00 AM,DPO:-94.5066666666667,-94.5066666666667,-106.56
6/6/2012 12:00:00 AM,DPO:-85.2133333333336,-85.2133333333331,-95.5066666666667
6/7/2012 12:00:00 AM,DPO:-94.7066666666669,-94.7066666666665,-101.213333333333
6/8/2012 12:00:00 AM,DPO:-107.253333333334,-107.253333333333,-112.106666666667

DPO == d1 in OpenQuant,but the d2 is the right result.
*/

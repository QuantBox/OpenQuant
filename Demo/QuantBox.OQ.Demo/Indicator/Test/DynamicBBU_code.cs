using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator.Test
{
    public class DynamicBBU_code : Strategy
    {
        DynamicBBU dbbu;
        TimeSeries lengths = new TimeSeries("Lengths");

        public override void OnStrategyStart()
        {
            dbbu = new DynamicBBU(lengths, Bars, 2, BarData.Close);

            Draw(dbbu, 0);
            Draw(dbbu.BBL, 0);
            Draw(dbbu.SMA, 0);
            Draw(dbbu.VAR, 2);

            Draw(lengths, 3);
        }



        public override void OnBar(Bar bar)
        {

            // 这个地方是关键
            lengths.Add(bar.DateTime, 5);

            // 别的就自己写吧

        }
    }
}

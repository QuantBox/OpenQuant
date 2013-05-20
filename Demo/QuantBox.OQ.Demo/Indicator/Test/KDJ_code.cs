using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator.Test
{
    public class KDJ_code : Strategy
    {
        KDJ kdj;
        HorizontalLine line80;
        HorizontalLine line20;

        public override void OnStrategyStart()
        {
            kdj = KDJ.create(Bars, 10, BarData.Close);
            kdj.Color = Color.Green;
            kdj.K.Color = Color.White;
            kdj.D.Color = Color.Yellow;
            Draw(kdj, 2);
            Draw(kdj.K, 2);
            Draw(kdj.D, 2);

            line80 = new HorizontalLine(Bars, 80);
            line80.Color = Color.Red;
            Draw(line80, 2);

            line20 = new HorizontalLine(Bars, 20);
            line20.Color = Color.Green;
            Draw(line20, 2);
        }
    }
}

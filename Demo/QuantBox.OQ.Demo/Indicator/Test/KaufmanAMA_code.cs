using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator.Test
{
    public class KaufmanAMA_code : Strategy
    {
        int N = 8;
        int SL = 2;
        int FS = 20;

        KaufmanAMA ama;
        SMA sma;

        public override void OnStrategyStart()
        {
            ama = new KaufmanAMA(Bars, N, SL, FS);
            sma = new SMA(Bars, N, Color.Red);

            Draw(ama, 0);
            Draw(sma, 0);
            Draw(ama.ER, 2);
        }
    }
}

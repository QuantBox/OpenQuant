using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;


namespace QuantBox.OQ.Demo.Indicator
{
    /// <summary>
    /// 乖离率
    /// </summary>
    public class BIAS : UserIndicator
    {
        SMA sma;
        int length;
        BarData barData;

        public static BIAS create(BarSeries series, int length, BarData barData)
        {
            SMA sma = new SMA(series, length);
            return new BIAS(series, sma, length, barData);
        }

        protected BIAS(BarSeries series, SMA sma, int length, BarData barData)
            : base(series)
        {
            this.length = length;
            this.sma = sma;
            this.barData = barData;
            this.Name = "BIAS";
        }

        public override double Calculate(int index)
        {
            int i = index - length + 1;
            if (i < 0)
                return double.NaN;

            double C = Input[index, barData];

            return (C - sma[i]) * 100 / sma[i];
        }
    }
}

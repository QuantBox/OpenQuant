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
    /// 动态时间窗口布林线
    /// </summary>
    public class DynamicBBU : UserIndicator
    {
        public TimeSeries SMA = new TimeSeries("SMA");
        public TimeSeries BBL = new TimeSeries("BBL");
        public TimeSeries VAR = new TimeSeries("VAR");

        private BarSeries bars;
        private BarData barData;
        private double k;

        public DynamicBBU(ISeries series, BarSeries bars, double k, BarData barData)
            : base(series)
        {
            this.k = k;
            this.bars = bars;
            this.barData = barData;
            this.Name = "DynamicBBU";
        }

        public override double Calculate(int index)
        {

            DateTime datetime = Input.GetDateTime(index);
            if (null == datetime)
                return double.NaN;

            int length = (int)Input[index, barData];
            int _index = bars.GetIndex(datetime);
            if (_index < 0)
                return double.NaN;

            double sum = 0;
            int count = 0;
            for (int j = _index; j >= _index - length + 1 && j >= 0; --j)
            {
                sum += bars[j, barData];
                ++count;
            }

            double sma = sum / count;
            SMA.Add(datetime, sma);

            double dsum = 0;
            int dcount = 0;
            for (int j = _index; j >= _index - length + 1 && j >= 0; --j)
            {
                dsum += Math.Pow((bars[j, barData] - sma), 2);
                ++dcount;
            }

            double se = Math.Sqrt(dsum / dcount);
            VAR.Add(datetime, se);
            BBL.Add(datetime, sma - k * se);

            return sma + k * se;
        }
    }
}

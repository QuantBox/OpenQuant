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
    /// KDJ指标
    /// </summary>
    public class KDJ : UserIndicator
    {
        int length;
        K_Fast rsv;
        public TimeSeries K;
        public TimeSeries D;

        public static KDJ create(BarSeries series, int length, BarData barData)
        {
            K_Fast rsv = new K_Fast(series, length);
            return new KDJ(series, rsv, length);
        }

        protected KDJ(BarSeries series, K_Fast rsv, int length)
            : base(series)
        {
            this.length = length;
            this.rsv = rsv;
            this.K = new TimeSeries("K");
            this.D = new TimeSeries("D");
            this.Name = "KDJ(" + length + ")";
        }

        public override double Calculate(int index)
        {
            int i = index - length + 1;
            if (i == 0)
            {
                // 将前一天初始化为50
                DateTime dt1 = Input.GetDateTime(index - 1);
                K.Add(dt1, 50);
                D.Add(dt1, 50);
            }
            else if (i < 0)
                return double.NaN;

            double KValue = K[i];
            double DValue = D[i];

            KValue = (2.0 * KValue + rsv[i]) / 3.0;
            DValue = (2.0 * DValue + KValue) / 3.0;

            DateTime dt = Input.GetDateTime(index);

            K.Add(dt, KValue);
            D.Add(dt, DValue);

            return 3 * KValue - 2 * DValue;
        }
    }
}

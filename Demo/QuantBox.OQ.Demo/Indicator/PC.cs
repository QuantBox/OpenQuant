using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator
{
    /// <summary>
    /// 增强版价格通道
    /// </summary>
    public class PC : UserIndicator
    {
        public enum CalcType
        {
            Max,
            Min,
        }

        public enum UseLast
        {
            Yes,
            No,
        }

        private BarData barData;
        private int length;
        private CalcType calcType;
        private UseLast useLast;

        public PC(ISeries series, int length, BarData barData, CalcType calcType, UseLast useLast)
            : base(series)
        {
            this.length = length;
            this.barData = barData;
            this.calcType = calcType;
            this.useLast = useLast;
            this.Name = "PriceChannel";
        }

        public override double Calculate(int index)
        {
            int _index = index - length;
            if (_index < 0)
            {
                return double.NaN;
            }

            if (calcType == CalcType.Max)
            {
                double max = double.MinValue;
                for (int i = _index; i < index; ++i)
                {
                    max = Math.Max(max, Input[i, barData]);
                }
                if (UseLast.Yes == useLast)
                {
                    max = Math.Max(max, Input[index, barData]);
                }
                return max;
            }
            else
            {
                double min = double.MaxValue;
                for (int i = _index; i < index; ++i)
                {
                    min = Math.Min(min, Input[i, barData]);
                }
                if (UseLast.Yes == useLast)
                {
                    min = Math.Min(min, Input[index, barData]);
                }
                return min;
            }
        }
    }
}

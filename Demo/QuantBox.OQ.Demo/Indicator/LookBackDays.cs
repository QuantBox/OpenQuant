using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator
{
    /// <summary>
    /// 计算可变窗口
    /// </summary>
    public class LookBackDays : UserIndicator
    {
        private double lookBackDays = 0;
        private int floorAmt = 20;
        private int ceilingAmt = 60;

        public LookBackDays(ISeries series, int lookBackDays, int floorAmt, int ceilingAmt)
            : base(series)
        {
            this.lookBackDays = lookBackDays;
            this.floorAmt = floorAmt;
            this.ceilingAmt = ceilingAmt;
            this.Name = "LookBackDays";
        }

        public override double Calculate(int index)
        {
            if (index - 1 < 0)
                return double.NaN;

            double todayVolatility = Input[index, BarData.Close];
            double yesterdayVolatility = Input[index - 1, BarData.Close];
            double deltaVolatility = (todayVolatility - yesterdayVolatility) / todayVolatility;
            lookBackDays = lookBackDays * (1.0 + deltaVolatility);
            lookBackDays = Math.Min(lookBackDays, ceilingAmt);
            lookBackDays = Math.Max(lookBackDays, floorAmt);

            return Math.Round(lookBackDays, 0);
        }
    }
}

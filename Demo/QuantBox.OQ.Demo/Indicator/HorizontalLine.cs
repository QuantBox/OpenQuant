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
    /// 水平线
    /// </summary>
    public class HorizontalLine : UserIndicator
    {
        double level;

        public HorizontalLine(ISeries series, int level)
            : base(series)
        {
            this.level = level;
            this.Name = string.Format("HorizontalLine({0})", level);
        }

        public override double Calculate(int index)
        {
            return level;
        }
    }
}

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// 在K线上将每天的第一个K线上画点，标记出来
    /// </summary>
    public class DrawCircles_code : Strategy
    {
        int day = -1;
        public TimeSeries ts = new TimeSeries();

        public override void OnStrategyStart()
        {
            ts.Width = 5;
            Draw(ts, 0, DrawStyle.Circles);
        }

        public override void OnBar(Bar bar)
        {
            if (bar.DateTime.Day != day)
            {
                day = bar.DateTime.Day;//获取bqr序列的日期
                ts.Add(bar.DateTime, bar.High + 1);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Data
{
    /// <summary>
    /// 使用非自然时间的Bar
    /// 要
    /// 
    /// 解决方案来源于英文官网
    /// http://www.smartquant.com/forums/viewtopic.php?f=64&t=7776
    /// </summary>
    public class Non_natural_TimeBar_code : Strategy
    {
        //临时Bar序列
        private BarSeries bars = null;
        private BarSeries bars30min = new BarSeries();

        /*
        按照股指的时间要求，时间划分是这样的
        9:15
        9:45
        10:15
        10:45
        11:15-11:30 13:00-13:15 两个15分钟被午休隔开了
        13:45
        14:15
        14:45
        15:15 交割日时只到15:00,已经到最后一天了，少15分钟也没什么
        */
        public override void OnBar(Bar bar)
        {
            //只处理15分钟的
            if (900 == bar.Size)
            {
                if (bars == null)
                    bars = new BarSeries();

                bars.Add(bar);

                //在处理11:15-11:30 13:00-13:15这两个15分钟时会合并成一个
                if (bars.Count == 2) // 2 * 15min = 30 min
                {
                    // get OHLC values for 30min bar
                    double open = bars[0].Open;
                    double high = bars.HighestHigh();
                    double low = bars.LowestLow();
                    double close = bars[1].Close;
                    long volume = bars[0].Volume + bars[1].Volume;

                    // todo something
                    Bar b = new Bar(bars[0].DateTime, open, high, low, close, volume, 900 * 2);
                    bars30min.Add(b);
                    Console.WriteLine(b);

                    // reset 15min bar series
                    bars = null;
                }
            }
        }
    }
}

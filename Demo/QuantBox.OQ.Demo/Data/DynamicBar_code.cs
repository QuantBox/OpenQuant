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
    /// 实时更新的Bar
    /// 
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=118
    /// 
    /// 注:貌似在OnQuote中直接引用Bar.High，Bar.Low，他会根据新的数据进行更新。
    /// </summary>
    public class DynamicBar_code:Strategy
    {
        double dbOpen = double.NaN;
        double dbHigh = double.MinValue;
        double dbLow = double.MaxValue;
        double dbClose = double.NaN;
        double dbVolume = 0;

        public override void OnTrade(Trade trade)
        {
            //每次在行情到来时更新最高、最低和成交量
            dbHigh = Math.Max(trade.Price, dbHigh);
            dbLow = Math.Min(trade.Price, dbLow);
            dbClose = trade.Price;
            dbVolume += trade.Size;

            //其他代码
        }

        public override void OnBarOpen(Bar bar)
        {
            //因为先OnTrade来后才OnBarOpen，
            //如果只在OnBarOpen中更新高低与量，
            //这时在OnTrade中计算的高低与量并不准确，
            //所以要提前到前一个Bar在OnBar时更新数据
            dbOpen = bar.Close;
        }

        public override void OnBar(Bar bar)
        {
            // 其他代码,用完后再重置

            // 重置
            if (180 == bar.Size)
            {
                dbHigh = double.MinValue;
                dbLow = double.MaxValue;
                dbVolume = 0;
            }
        }
    }
}

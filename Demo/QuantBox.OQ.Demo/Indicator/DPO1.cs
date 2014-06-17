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
    /// 自己的DPO指标
    /// </summary>
    public class DPO1 : UserIndicator
    {
        private BarData option;
        private int length;

        public DPO1(ISeries series, int length, BarData barData)
            : base(series)
        {
            this.length = length;//长度
            this.option = barData;//bar类型
            this.Name = "DPO";//名称
        }

        public override double Calculate(int index)
        {
            if (index > length / 2 + length - 1)
            {
                double price = Input[index, option];
                double sum = 0;

                //for(int i = index - length / 2; i >index - length - length / 2; i--)
                for (int i = index - length / 2 - 1; i >= index - length - length / 2; i--)
                    sum += Input[i, option];

                sum /= length;

                double DPO = price - sum;

                return DPO;
            }
            else
                return double.NaN;
        }
    }
}

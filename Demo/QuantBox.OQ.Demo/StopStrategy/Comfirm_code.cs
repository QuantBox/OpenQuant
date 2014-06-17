using System;
using System.Drawing;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;

namespace QuantBox.OQ.Demo.StopStrategy
{
    public class MyStrategy : Strategy
    {
        bool bStopStrategy = false;
        public override void OnStrategyStart()
        {
            DialogResult dr = MessageBox.Show("是否停止？", "确认", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                //1.直接停止。其实写在这并不能停止策略
                //StopStrategy();

                //2.使用定时器，3秒后停止
                AddTimer(Clock.Now.AddSeconds(3));

                //3.使用变量，在策略运行的地方停止
                //bStopStrategy = true;
            }
        }

        public override void OnTimer(DateTime datetime, Object data)
        {
            Console.WriteLine("OnTimer");
            StopStrategy();
        }

        public override void OnBar(Bar bar)
        {
            if (bStopStrategy)
            {
                Console.WriteLine("OnBar");
                StopStrategy();
            }

            Console.WriteLine(bar);
        }

    }
}
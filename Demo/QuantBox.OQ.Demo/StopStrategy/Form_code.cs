using System;
using System.Drawing;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;

namespace QuantBox.OQ.Demo.StopStrategy
{
    public class Form1 : Form
    {
        public bool bStopStrategy = false;
        public Button button1;
        public Form1()
        {
            button1 = new Button();
            button1.Size = new Size(40, 40);
            button1.Location = new Point(30, 30);
            button1.Text = "Click me";
            this.Controls.Add(button1);
            button1.Click += new EventHandler(button1_Click);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello World");
            bStopStrategy = true;
        }
    }

    /// <summary>
    /// 弹出窗口，在窗口上点击按钮后停止
    /// 
    /// 或在User Commands中停止
    /// </summary>
    public class Form_code:Strategy
    {
        private Form1 test;

        public override void OnStrategyStart()
        {
            test = new Form1();
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                Application.Run(test);
            });
        }

        public override void OnBar(Bar bar)
        {
            if (test.bStopStrategy)
            {
                Console.WriteLine("OnBar");
                StopStrategy();
            }

            Console.WriteLine(bar);
        }

        public override void OnUserCommand(UserCommand command)
        {
            StopStrategy();
        }
    }
}

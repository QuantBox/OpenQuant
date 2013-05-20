using System;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Engine;

namespace QuantBox.OQ.Demo.StopStrategy
{
    public class Comfirm_Scenario : Scenario
    {
        public override void Run()
        {
            DialogResult dr = MessageBox.Show("是否继续？", "确认", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                Start();
            }
        }
    }
}

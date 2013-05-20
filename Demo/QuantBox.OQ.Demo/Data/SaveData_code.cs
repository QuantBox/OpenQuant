using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;

namespace QuantBox.OQ.Demo.Data
{
    /// <summary>
    /// 使用策略来保存行情
    /// 
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=24
    /// </summary>
    public class SaveData_code:Strategy
    {
        public override void OnTrade(Trade trade)
        {
            // 模拟行情时,不保存
            if (StrategyMode.Simulation != Mode)
                DataManager.Add(Instrument, trade);
        }

        public override void OnQuote(Quote quote)
        {
            if (StrategyMode.Simulation != Mode)
                DataManager.Add(Instrument, quote);
        }
    }
}

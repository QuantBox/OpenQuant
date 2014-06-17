using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using QuantBox.OQ.Demo.Module;
using QuantBox.OQ.Demo.Helper;
using QuantBox.OQ.Extensions.OrderText;

namespace QuantBox.OQ.Demo.Strategys
{
    public class PairTrading2_code : TargetPositionModule
    {
        // 自配的合约，行情可以由价差生成功能提供
        public const string Symbol0 = "AAPL";
        // 交易所合约
        public const string Symbol1 = "MSFT";
        public const string Symbol2 = "DELL";

        [OptimizationParameter(5, 10, 1)]
        [Parameter("快均线", "SMA")]
        public int fastLength = 5;

        [OptimizationParameter(11, 15, 1)]
        [Parameter("慢均线", "SMA")]
        public int slowLength = 12;

        SMA fastSMA;
        SMA slowSMA;

        // 直接使用这种static比使用Global要快
        static PairTrading2_code S1;
        static PairTrading2_code S2;
        
        public override void OnStrategyStart()
        {
            base.OnStrategyStart();

            //测试用，自定义交易时间,只使用日线做简单测试时极方便
            base.TimeHelper = new TimeHelper(new int[] { 0, 2400 }, 2100,1700);
            // 此处要测试是否支持交易所组合合约
            base.TextParameter = new TextSP();

            base.TargetPosition = 0;
            base.DualPosition.Long.Qty = 0;
            base.DualPosition.Short.Qty = 0;

            // 记下策略的对象,在其它实例中可以用到
            //Global.Add(Instrument.Symbol, this);

            if (Instrument.Symbol == Symbol1)
            {
                S1 = this;
            }
            if (Instrument.Symbol == Symbol2)
            {
                S2 = this;
            }

            fastSMA = new SMA(Bars, fastLength, Color.Red);
            slowSMA = new SMA(Bars, slowLength, Color.Green);

            Draw(fastSMA, 0);
            Draw(slowSMA, 0);
        }

        public override double GetCurrentQty()
        {
            if (Instrument.Symbol == Symbol0)
            {
                // 各项处理时使用信号持仓
                return TargetPosition;
            }
            else
            {
                // 各项处理时使用实盘持仓
                return DualPosition.NetQty;
            }
        }

        public override double GetLongAvgPrice()
        {
            if (Instrument.Symbol == Symbol0)
            {
                return S1.DualPosition.Long.AvgPrice - S2.DualPosition.Short.AvgPrice;
            }
            else
            {
                // 各项处理时使用实盘持仓
                return base.GetLongAvgPrice();
            }
        }

        public override double GetShortAvgPrice()
        {
            if (Instrument.Symbol == Symbol0)
            {
                return S1.DualPosition.Short.AvgPrice - S2.DualPosition.Long.AvgPrice;
            }
            else
            {
                // 各项处理时使用实盘持仓
                return base.GetLongAvgPrice();
            }
        }

        private void OnBar_Symbol0(Bar bar)
        {
            do
            {
                Cross cross = fastSMA.Crosses(slowSMA, bar);
                if (Cross.Above == cross)
                {
                    base.TargetPosition = 1;
                    TextParameter.Text = "金叉";
                }
                else if (Cross.Below == cross)
                {
                    base.TargetPosition = -1;
                    TextParameter.Text = "死叉";
                }
                else
                {
                    // 保持上次的状态
                }

                // 合成合约没有持仓等基本信息，如何进行止损呢？
                FixedStop(bar.Close, 300, StopMode.Absolute, "");
                TakeProfit(bar.Close, 400, StopMode.Absolute, "");

            } while (false);
        }

        public override void OnBar(Bar bar)
        {
            // 合理设置合约的属性，可以减少字符串比较
            //if(Instrument.Type == InstrumentType.MultiLeg)
            if (Instrument.Symbol == Symbol0)
            {
                OnBar_Symbol0(bar);
            }

            base.ChangeTradingDay();

            base.OnBar(bar);
        }

        public override void Process()
        {
            // 多腿合约，非实际存在的合约，不进行实际下单操作
            //if(Instrument.Type == InstrumentType.MultiLeg)
            if (Instrument.Symbol == Symbol0)
            {
                // 如果类名不同，此处得修改
                //PairTrading2_code S1 = Global[Symbol1] as PairTrading2_code;
                //PairTrading2_code S2 = Global[Symbol2] as PairTrading2_code;

                S1.TextParameter.Text = base.TextParameter.Text;
                S2.TextParameter.Text = base.TextParameter.Text;

                // 按配比，调整每个合约的下单
                S1.TargetPosition = base.TargetPosition;
                S2.TargetPosition = -base.TargetPosition;

                // 加锁，这个地方要测试下是不是支持4.0插件的交易所组合合约功能
                // 回测时将Latency设成1显示更直观
                lock(this.ExecutionProvider)
                {
                    S1.Process();
                    S2.Process();
                }
            }
            else
            {
                base.Process();
            }
        }
    }
}

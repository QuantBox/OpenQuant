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
    /// R-Breaker
    /// 
    /// 使用方法，添加一个日线，添加一个1分钟线
    /// 将第一天前一天的收盘、最高、最低填好
    /// 由于历史的收高低是手工填写的，所以整个代码中没有加载历史数据的代码，
    /// 如果想长期无人值守，只要在点运行前改正好上一交易日的收高低即可
    /// 
    /// 网上金字塔版，带图
    /// http://www.yafco.com/show.php?contentid=261740
    /// 论坛上的TB版
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=194
    /// </summary>
    public class RBreaker_code : Strategy
    {
        //第一次运行时，前一天的开高底收
        [Parameter("前一天最高价", "PreDay")]
        double preDayHigh = 0;
        [Parameter("前一天最低价", "PreDay")]
        double preDayLow = 0;
        [Parameter("前一天收盘价", "PreDay")]
        double preDayClose = 0;

        [OptimizationParameter(0.3, 0.5, 0.05)]
        [Parameter]
        double f1 = 0.35;
        [Parameter]
        [OptimizationParameter(0.05, 0.09, 0.01)]
        double f2 = 0.07;
        [Parameter]
        [OptimizationParameter(0.10, 0.15, 0.01)]
        double f3 = 0.12;
        [Parameter]
        double Qty = 1;

        double reverse = 0.5;
        double rangemin = 0.2;
        double xdiv = 3;

        double div = 3;
        double i_reverse = 1;
        double i_rangemin = 1;
        bool rfilter;

        TimeSeries S1;
        TimeSeries B1;

        [Parameter]
        int notbef = 900;
        [Parameter]
        int notaft = 1500;

        long barSize = long.MaxValue;

        #region R-Breaker
        TimeSeries ssetup;
        TimeSeries bsetup;
        TimeSeries senter;
        TimeSeries benter;
        TimeSeries bbreak;
        TimeSeries sbreak;

        double _ssetup = double.NaN;
        double _bsetup = double.NaN;
        double _senter = double.NaN;
        double _benter = double.NaN;
        double _bbreak = double.NaN;
        double _sbreak = double.NaN;

        void UpdateAtDaily_RBreaker(Bar bar)
        {
            preDayHigh = bar.High;
            preDayLow = bar.Low;
            preDayClose = bar.Close;

            Update6Line();
        }

        void Update6Line()
        {
            //先计算，时候到了再放到序列中
            _ssetup = preDayHigh + f1 * (preDayClose - preDayLow);
            _bsetup = preDayLow - f1 * (preDayHigh - preDayClose);

            _senter = (1 + f2) / 2.0 * (preDayHigh + preDayClose) - f2 * preDayLow;
            _benter = (1 + f2) / 2.0 * (preDayLow + preDayClose) - f2 * preDayHigh;

            _bbreak = _ssetup + f3 * (_ssetup - _bsetup);
            _sbreak = _bsetup - f3 * (_ssetup - _bsetup);
        }

        void UpdateAtBar_RBreaker(Bar bar)
        {
            ssetup.Add(bar.DateTime, _ssetup);
            bsetup.Add(bar.DateTime, _bsetup);
            senter.Add(bar.DateTime, _senter);
            benter.Add(bar.DateTime, _benter);
            bbreak.Add(bar.DateTime, _bbreak);
            sbreak.Add(bar.DateTime, _sbreak);
        }
        #endregion

        #region HighLow
        double HighToday = double.MinValue;
        double LowToday = double.MaxValue;
        void UpdateAtDaily_HighLow(Bar bar)
        {
            HighToday = double.MinValue;
            LowToday = double.MaxValue;
        }
        void UpdateAtBar_HighLow(Bar bar)
        {

            HighToday = Math.Max(HighToday, bar.High);
            LowToday = Math.Min(LowToday, bar.Low);
        }
        #endregion

        public override void OnStrategyStart()
        {
            // 自动得到当时时间窗口大小
            foreach (BarRequest barRequest in DataRequests.BarRequests)
            {
                barSize = Math.Min(barSize, barRequest.BarSize);
            }
            Console.WriteLine("barSize = {0}", barSize);

            Update6Line();

            ssetup = new TimeSeries("观察卖出价", Color.Green);
            bsetup = new TimeSeries("观察买入价", Color.Green);
            bbreak = new TimeSeries("突破买入价", Color.Red);
            sbreak = new TimeSeries("突破卖出价", Color.Red);
            senter = new TimeSeries("反转卖出价", Color.Black);
            benter = new TimeSeries("反转买入价", Color.Black);

            S1 = new TimeSeries("S1", Color.Blue);
            B1 = new TimeSeries("B1", Color.Blue);

            Draw(ssetup, 0);
            Draw(bsetup, 0);
            Draw(bbreak, 0);
            Draw(sbreak, 0);
            Draw(senter, 0);
            Draw(benter, 0);

            Draw(S1, 0);
            Draw(B1, 0);
        }

        public override void OnBarOpen(Bar bar)
        {
            if (86400 == bar.Size)
            {
                i_reverse = reverse * (bar.Open / 100.0);
                i_rangemin = rangemin * (bar.Open / 100.0);
                div = Math.Max(xdiv, 1);
            }
        }

        public override void OnBar(Bar bar)
        {
            if (86400 == bar.Size)
            {
                //表示今天结束了，要记下今天的开高底收和明天可以用到的价格
                UpdateAtDaily_RBreaker(bar);
                UpdateAtDaily_HighLow(bar);

                rfilter = (preDayHigh - preDayLow) >= i_rangemin;
                return;
            }

            UpdateAtBar_RBreaker(bar);
            UpdateAtBar_HighLow(bar);

            double _S1 = _senter + (HighToday - _ssetup) / div;
            double _B1 = _benter - (_bsetup - LowToday) / div;

            S1.Add(bar.DateTime, _S1);
            B1.Add(bar.DateTime, _B1);


            int nDateTime = Clock.Now.Hour * 100 + Clock.Now.Minute;
            if (nDateTime < notbef)
            {
                return;
            }
            if (nDateTime > notaft)
            {
                ClosePosition("T|");
                return;
            }

            if (HasPosition)
            {
                if (Position.Amount > 0)
                {
                    if ((HighToday > _ssetup && bar.Close < _S1)
                            || bar.Close < _sbreak)
                    {
                        string text = string.Format("{0}>{1}&&{2}<{3}",
                                HighToday, _ssetup,
                                bar.Close, _S1);
                        ClosePosition("T|" + text);
                        Sell(Qty, "O|" + text);
                    }
                }
                else
                {
                    if ((LowToday < _bsetup && bar.Close > _B1)
                            || bar.Close > _bbreak)
                    {
                        string text = string.Format("{0}>{1}&&{2}<{3}",
                                LowToday, _bsetup,
                                bar.Close, _B1);
                        ClosePosition("T|" + text);
                        Buy(Qty, "O|" + text);
                    }
                }
            }
            else
            {
                if (bar.Close > _bbreak)
                {
                    Buy(Qty, string.Format("O|{0}>{1}", bar.Close, _bbreak));
                }
                if (bar.Close < _sbreak)
                {
                    Sell(Qty, string.Format("O|{0}<{1}", bar.Close, _bbreak));
                }
            }
        }
    }

}

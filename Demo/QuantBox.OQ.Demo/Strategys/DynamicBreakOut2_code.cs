using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

using QuantBox.OQ.Demo.Indicator;

namespace QuantBox.OQ.Demo.Strategys
{
    /// <summary>
    /// 自适应动态突破系统
    /// 
    /// 添加一个日线，再添加一个1分钟
    /// 
    /// 论坛上的TB代码
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=193
    /// 网上的TS代码
    /// http://www.programtrading.tw/viewtopic.php?t=504
    /// </summary>
    public class DynamicBreakOut2_code : Strategy
    {
        [Parameter("指标窗口长度（LookBackDays）的上限")]
        int ceilingAmt = 60;
        [Parameter("指标窗口长度（LookBackDays）的下限")]
        int floorAmt = 20;
        [Parameter("布林线参数")]
        double bolBandTrig = 2;
        [Parameter("合约数目")]
        double Qty = 1;

        // 指标窗口长度（LookBackDays）的初始值
        int lookBackDays = 20;

        SMD smd30;
        LookBackDays lbd;
        DynamicBBU dbbu;

        BarSeries bars86400;

        TimeSeries upBandSeries;
        TimeSeries dnBandSeries;
        TimeSeries buyPointSeries;
        TimeSeries sellPointSeries;

        public override void OnStrategyStart()
        {
            bars86400 = GetBars(BarType.Time, 86400);

            smd30 = new SMD(bars86400, 30);
            lbd = new LookBackDays(smd30, lookBackDays, floorAmt, ceilingAmt);
            dbbu = new DynamicBBU(lbd, bars86400, bolBandTrig, BarData.Close);

            Draw(smd30, 2);
            Draw(lbd, 3);
            Draw(dbbu.SMA, 0);
            Draw(dbbu.BBL, 0);
            Draw(dbbu, 0);

            upBandSeries = new TimeSeries("upBandSeries");
            dnBandSeries = new TimeSeries("dnBandSeries");
            buyPointSeries = new TimeSeries("buyPointSeries");
            sellPointSeries = new TimeSeries("sellPointSeries");

            upBandSeries.Color = Color.Red;
            dnBandSeries.Color = Color.Red;

            Draw(upBandSeries, 0);
            Draw(dnBandSeries, 0);
            Draw(buyPointSeries, 0);
            Draw(sellPointSeries, 0);
        }

        public override void OnBar(Bar bar)
        {
            if (bar.Size == 86400)
                return;

            if (lbd.Count < 1 || dbbu.Count < 1)
                return;

            int lookBackDaysInt = (int)lbd.Last;
            int nEnd = bars86400.Count - 1;
            int nBegin = nEnd - lookBackDays + 1;

            double buyPoint = bars86400.HighestHigh(nBegin, nEnd);
            double sellPoint = bars86400.LowestLow(nBegin, nEnd);
            double longLiqPoint = dbbu.SMA.Last;
            double shortLiqPoint = dbbu.SMA.Last;
            double upBand = dbbu.Last;
            double dnBand = dbbu.BBL.Last;

            upBandSeries.Add(bar.DateTime, upBand);
            dnBandSeries.Add(bar.DateTime, dnBand);
            buyPointSeries.Add(bar.DateTime, buyPoint);
            sellPointSeries.Add(bar.DateTime, sellPoint);

            //  下面代码可能有问题
            if (HasPosition)
            {
                if (Position.Amount > 0 && Bar.Close < longLiqPoint)
                {
                    ClosePosition("T|");
                }
                if (Position.Amount < 0 && Bar.Close > shortLiqPoint)
                {
                    ClosePosition("T|");
                }
            }
            else
            {
                if (Bar.Close > upBand)// && Bar.Close >= buyPoint
                {
                    Buy(Qty, "O|");
                }
                if (Bar.Close < dnBand)// && Bar.Close <= sellPoint
                {
                    Sell(Qty, "O|");
                }
            }
        }
    }
}

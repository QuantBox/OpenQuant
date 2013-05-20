using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Engine;

namespace QuantBox.OQ.Demo.Optimization
{
    /// <summary>
    /// 计算率夏普比率
    /// </summary>
    public class SharpeRatio_Scenario : Scenario
    {
        public override void Run()
        {
            Start();

            //年无风险利率
            double riskFreeRate = 0.03;
            double SR = SharpeRatio(riskFreeRate);
            Console.WriteLine(SR);
        }

        public double SharpeRatio(double RiskFreeRate)
        {
            int Days = Solution.Performance.PnLSeries.Count;
            TimeSeries ReturnSeries = new TimeSeries();
            double Portfolio0 = Solution.Cash;
            double Portfolio1;
            for (int t = 0; t < Days; ++t)
            {
                DateTime date = Solution.Performance.PnLSeries.GetDateTime(t);
                Portfolio1 = Solution.Performance.PnLSeries[t] + Portfolio0;
                double DailyReturn = Portfolio1 / Portfolio0 - 1;
                Portfolio0 = Portfolio1;
                ReturnSeries.Add(date, DailyReturn);
            }

            double Mean = ReturnSeries.GetMean(0, Days - 1);
            double Std = ReturnSeries.GetStdDev(0, Days - 1);

            //年化收益均值、波动率
            Mean = Mean * 250;
            Std = Std * Math.Sqrt(250);

            double SR = (Mean - RiskFreeRate) / Std;
            return SR;
        }
    }
}

/*
夏普比率公式：
Sharpe Ratio = (Mean Return - Risk Free Rate)/Volatility
Mean Return是收益率的均值
Volatility是收益率的标准差
Risk Free Rate是无风险利率

使用方法
1).
Tools->Options->Memory Management
将Enable built-in porfolio performance和Enable interval performance update选上，
然后将Interval length改成86400

2)
代码
先得出日收益率序列ReturnSeries，然后求收益率序列的均值、标准差 


资产组合或者说策略的收益率可以有两种，一是对数收益率，二是简单收益率
对数收益率 = log( Price(t) / Price(t-1) ) = log(Price(t)) -  log(Price(t-1)) 
简单收益率 = Price(t) / Price(t-1) - 1
数学计算时常用对数收益率，因为对数收益率有两个非常好的特性：
1）一年的日对数收益率之和刚好是年对数收益率，而普通收益率就没有这个特性
2）此外对数收益率的好处就是涨跌10%对应在价格上是一样的，用简单收益率的话，100元的股票，跌10%变成了90元，再涨10%只能是99元，而不是100元，相反对数收益率就不存在这个问题。

所以很多时候用对数收益率好一些，但是这里其实结果差别不大就后来又用了普通收益率
 */

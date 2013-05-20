using System;
using System.Drawing;

using OpenQuant.API;
using OpenQuant.API.Indicators;

namespace QuantBox.OQ.Demo.Data
{
    /// <summary>
    /// 通过策略运行的方式来生成价差序列
    /// </summary>
    public class DataMaker_code : Strategy
    {
        static Instrument Instrument1;
        static Instrument Instrument2;
        Instrument Instrument3 = InstrumentManager.Instruments["IF1305 - IF1306"];

        long barSize;

        public override void OnStrategyStart()
        {
            if (Instrument1 == null)
            {
                Instrument1 = Instrument;
            }
            else if (Instrument2 == null)
            {
                Instrument2 = Instrument;
            }

            barSize = long.MaxValue;
            foreach (BarRequest barRequest in DataRequests.BarRequests)
            {
                barSize = Math.Min(barSize, barRequest.BarSize);
            }
            Console.WriteLine("barSize = {0}", barSize);
        }

        public override void OnStrategyStop()
        {
            Instrument1 = null;
            Instrument2 = null;
        }

        public override void OnTrade(Trade trade)
        {
            // 只用第二个合约的生成，数量约为第二个合约的Trade数
            // 假如第一个是IF1309,第二个是399300.SZ，这下生成的就6秒一次了
            // 按自己需求调
            if (Instrument == Instrument2)
            {
                if (Instrument1.Trade != null
                        && Instrument2.Trade != null)
                {
                    double Price = Instrument1.Trade.Price - Instrument2.Trade.Price;
                    int Size = Math.Min(Instrument1.Trade.Size, Instrument2.Trade.Size);
                    Trade t = new Trade(Clock.Now, Price, Size);
                    DataManager.Add(Instrument3, t);
                }
            }
        }

        public override void OnQuote(Quote quote)
        {
            // 只要有报价就会生成，数量约为两个合约Quote之和 
            //if(Instrument == Instrument2)
            {
                if (Instrument1.Quote != null
                        && Instrument2.Quote != null)
                {
                    double Ask = Instrument1.Quote.Ask - Instrument2.Quote.Bid;
                    int AskSize = Math.Min(Instrument1.Quote.AskSize, Instrument2.Quote.BidSize);
                    double Bid = Instrument1.Quote.Bid - Instrument2.Quote.Ask;
                    int BidSize = Math.Min(Instrument1.Quote.BidSize, Instrument2.Quote.AskSize);
                    Quote q = new Quote(Clock.Now, Bid, BidSize, Ask, AskSize);
                    DataManager.Add(Instrument3, q);
                }
            }
        }

        public override void OnBarSlice(long size)
        {
            // 为了保证采样间隔一样，用户按自己的需求改
            if (size != barSize)
            {
                return;
            }

            // 如果添加了两个合约就会触发两次，只选后面一次保存
            if (Instrument == Instrument2)
            {
                // 本想保存成Bar,细想没必要,保存了Trade，用户自己手工压缩成Bar就成
                double Price = Instrument1.Bar.Close - Instrument2.Bar.Close;
                Trade t = new Trade(Clock.Now, Price, 0);
                // 注释了。在前面的OnTrade可以生成更细致的Trade
                //DataManager.Add(Instrument3,t);
            }
        }
    }

}

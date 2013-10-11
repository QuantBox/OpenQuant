using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class PriceHelper
    {
        public double UpperLimitPrice { get; private set; }
        public double LowerLimitPrice { get; private set; }
        public double TickSize { get; private set; }

        public PriceHelper(double TickSize)
        {
            this.UpperLimitPrice = double.MaxValue;
            this.LowerLimitPrice = double.MinValue;
            this.TickSize = TickSize;
        }

        public PriceHelper(double UpperLimitPrice,
            double LowerLimitPrice,
            double TickSize)
        {
            this.UpperLimitPrice = UpperLimitPrice;
            this.LowerLimitPrice = LowerLimitPrice;
            this.TickSize = TickSize;
        }

        public int GetKeyByPrice(double price, OrderSide Side)
        {
            price = Math.Min(price, UpperLimitPrice);
            price = Math.Max(price, LowerLimitPrice);

            int index = (int)((Side == OrderSide.Buy) ? Math.Ceiling(price / TickSize) : Math.Floor(price / TickSize));
            return index;
        }

        public double GetPriceByKey(int key)
        {
            return key * TickSize;
        }

        public double FixPrice(double price, OrderSide Side)
        {
            return GetPriceByKey(GetKeyByPrice(price, Side));
        }

        public double GetMatchPrice(Strategy strategy, OrderSide side)
        {
            Quote quote = strategy.Quote;
            Trade trade = strategy.Trade;
            Bar bar = strategy.Bar;

            if (quote != null)
            {
                if (side == OrderSide.Buy)
                {
                    if (quote.Ask != 0)
                        return quote.Ask;
                }
                else
                {
                    if (quote.Bid != 0)
                        return quote.Bid;
                }
            }

            if (trade != null)
                if (trade.Price != 0)
                    return trade.Price;

            if (bar != null)
            {
                if (bar.Close != 0)
                    return bar.Close;

                if (bar.Open != 0)
                    return bar.Open;
            }

            return 0;
        }

        // 在对手价上加一定跳数
        public double GetMatchPrice(Strategy strategy, OrderSide side, double jump)
        {
            double price = GetMatchPrice(strategy, side);
            if (side == OrderSide.Buy)
            {
                price += jump * TickSize;
            }
            else
            {
                price -= jump * TickSize;
            }

            // 修正一下价格
            price = FixPrice(price, side);

            return price;
        }
    }
}

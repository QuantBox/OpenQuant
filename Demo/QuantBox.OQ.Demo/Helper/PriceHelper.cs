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
    }
}

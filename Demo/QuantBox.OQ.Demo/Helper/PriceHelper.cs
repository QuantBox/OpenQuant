using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 价格助手
    /// </summary>
    public class PriceHelper
    {
        /// <summary>
        /// 价格上限
        /// </summary>
        public double UpperLimitPrice { get; private set; }
        /// <summary>
        /// 价格下限
        /// </summary>
        public double LowerLimitPrice { get; private set; }
        /// <summary>
        /// Tick最小变量数量
        /// </summary>
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
        /// <summary>
        /// 获取价格水平，取TickSize的整数倍
        /// </summary>
        /// <param name="price">价格</param>
        /// <param name="Side">买卖方向</param>
        /// <returns>价格</returns>
        public int GetLevelByPrice(double price, OrderSide Side)
        {
            price = Math.Min(price, UpperLimitPrice);
            price = Math.Max(price, LowerLimitPrice);

            int index = (int)((Side == OrderSide.Buy) ? Math.Ceiling(price / TickSize) : Math.Floor(price / TickSize));
            return index;
        }
        /// <summary>
        /// 获得价格
        /// </summary>
        /// <param name="level">价格</param>
        /// <returns></returns>
        public double GetPriceByLevel(int level)
        {
            return level * TickSize;//TickSize为每一跳的价格
        }
        /// <summary>
        /// 修正价格水平
        /// </summary>
        /// <param name="price"></param>
        /// <param name="Side"></param>
        /// <returns></returns>
        public double FixPrice(double price, OrderSide Side)
        {
            return GetPriceByLevel(GetLevelByPrice(price, Side));
        }
        /// <summary>
        /// 获得匹配的价格
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="side"></param>
        /// <returns></returns>
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

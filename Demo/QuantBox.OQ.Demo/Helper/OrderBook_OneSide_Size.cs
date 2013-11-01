using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class OrderBook_OneSide_Size : IComparer<int>
    {
        public OrderSide Side;
        public SortedList<int, double> Grid;
        public PriceHelper PriceHelper;


        public OrderBook_OneSide_Size(OrderSide Side)
        {
            this.Side = Side;
            Grid = new SortedList<int, double>(this);
        }

        public int Compare(int x, int y)
        {
            return x.CompareTo(y) * (Side == OrderSide.Buy ? -1 : 1);
        }

        public void Set(double price, double size)
        {
            int key = PriceHelper.GetLevelByPrice(price, Side);
            if (size <= 0)
            {
                Grid.Remove(key);
            }
            else
            {
                Grid[key] = size;
            }
        }

        public void Clear()
        {
            Grid.Clear();
        }

        public int Count
        {
            get { return Grid.Count; }
        }

        public IEnumerable<KeyValuePair<int, double>> Intersect(OrderBook_OneSide_Size obs)
        {
            return this.Grid.Intersect(obs.Grid);
        }
    }
}

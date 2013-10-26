using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class OrderBook_OneSide_Size : IComparer<int>
    {
        private OrderSide Side;
        public SortedList<int, double> grid;
        public PriceHelper PriceHelper;


        public OrderBook_OneSide_Size(OrderSide Side)
        {
            this.Side = Side;
            grid = new SortedList<int, double>(this);
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
                grid.Remove(key);
            }
            else
            {
                grid[key] = size;
            }
        }

        public void Clear()
        {
            grid.Clear();
        }

        public int Count
        {
            get { return grid.Count; }
        }

        //public int abc()
        //{
        //    grid.k
        //}

        public IEnumerable<KeyValuePair<int, double>> Intersect(OrderBook_OneSide_Size obs)
        {
            return this.grid.Intersect(obs.grid);
        }
    }
}

using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class OrderBook_OneSide_Order : IComparer<int>
    {
        private OrderSide Side;
        public SortedList<int, HashSet<Order>> grid;
        public PriceHelper PriceHelper;

        public OrderBook_OneSide_Order(OrderSide Side)
        {
            this.Side = Side;
            grid = new SortedList<int, HashSet<Order>>(this);
        }

        public int Compare(int x, int y)
        {
            return x.CompareTo(y) * (Side == OrderSide.Buy ? -1 : 1);
        }

        public bool IsPending
        {
            get { return grid.Count > 0; }
        }


        public void Add(Order order)
        {
            HashSet<Order> set;
            int key = PriceHelper.GetLevelByPrice(order.Price, Side);
            if (!grid.TryGetValue(key, out set))
            {
                set = new HashSet<Order>();
                grid.Add(key, set);
            }
            set.Add(order);
        }

        public void Remove(Order order)
        {
            HashSet<Order> set;
            int key = PriceHelper.GetLevelByPrice(order.Price, Side);
            if (!grid.TryGetValue(key, out set))
            {
                return;
            }
            set.Remove(order);
            if (set.Count == 0)
            {
                grid.Remove(key);
            }
        }

        public double Size(HashSet<Order> set)
        {
            double sum = 0;
            foreach (var o in set)
            {
                sum += o.LeavesQty;
            }
            return sum;
        }

        public double Size()
        {
            double sum = 0;
            foreach(HashSet<Order> set in grid.Values)
            {
                sum += Size(set);
            }
            return sum;
        }

        public double SizeByIndex(int index = 0)
        {
            if (index < 0 || index >= grid.Count)
                return 0;

            HashSet<Order> set = grid.Values[index];
            return Size(set);
        }


        public double SizeByPrice(double price)
        {
            int key = PriceHelper.GetLevelByPrice(price, Side);
            HashSet<Order> set;
            if (!grid.TryGetValue(key, out set))
            {
                return 0;
            }

            return Size(set);
        }

        public int Cancel(HashSet<Order> set)
        {
            int cnt = 0;
            foreach (var o in set)
            {
                if (!o.IsDone)
                {
                    o.Cancel();
                    ++cnt;
                }
            }
            return cnt;
        }


        public double PriceByIndex(int index = 0)
        {
            if (index < 0 || index >= grid.Count)
                return 0;

            int key = grid.Keys[index];
            return PriceHelper.GetPriceByLevel(key);
        }

        public int CountLevel()
        {
            return grid.Count;
        }

        // 指定index的全撤
        public int CancelByIndex(int index)
        {
            int cnt = 0;
            if (index < 0 || index >= grid.Count)
                return 0;

            HashSet<Order> set = grid.Values[index];
            return Cancel(set);
        }

        public int CancelByOrderBook(OrderBook_OneSide_Size orderbook)
        {
            // 
            int cnt = 0;

            foreach(var a in grid)
            {
                double size = orderbook.grid[a.Key];

            }

            return cnt;
        }

        public int CancelExcludePrice(double price)
        {
            int cnt = 0;
            int key = PriceHelper.GetLevelByPrice(price, Side);
            foreach (int k in grid.Keys)
            {
                if (k == key)
                    continue;

                HashSet<Order> set;
                if (!grid.TryGetValue(k, out set))
                {
                    continue;
                }
                cnt += Cancel(set);
            }

            return cnt;
        }


        public override string ToString()
        {
            string str = "";
            foreach (var i in grid)
            {
                double price = PriceHelper.GetPriceByLevel(i.Key);
                double sum = 0;
                foreach (var o in i.Value)
                {
                    sum += o.LeavesQty;
                }
                str += string.Format("{0} {1}{2}", price, sum, Environment.NewLine);
            }
            return str;
        }
    }
}

using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class OrderBook_OneSide_Order : IComparer<int>
    {
        public OrderSide Side;
        public SortedList<int, HashSet<Order>> Grid;
        public PriceHelper PriceHelper;
        public HashSet<Order> cancelList = new HashSet<Order>();

        public OrderBook_OneSide_Order(OrderSide Side)
        {
            this.Side = Side;
            Grid = new SortedList<int, HashSet<Order>>(this);
        }

        public int Compare(int x, int y)
        {
            return x.CompareTo(y) * (Side == OrderSide.Buy ? -1 : 1);
        }

        public bool IsPending
        {
            get { return Grid.Count > 0; }
        }

        public void Clear()
        {
            Grid.Clear();
        }

        public int Count
        {
            get { return Grid.Count; }
        }


        public void Add(Order order)
        {
            lock(this)
            {
                HashSet<Order> set;
                int key = PriceHelper.GetLevelByPrice(order.Price, Side);
                if (!Grid.TryGetValue(key, out set))
                {
                    set = new HashSet<Order>();
                    Grid.Add(key, set);
                }
                set.Add(order);
            }
        }

        public void Remove(Order order)
        {
            lock (this)
            {
                HashSet<Order> set;
                int key = PriceHelper.GetLevelByPrice(order.Price, Side);
                if (!Grid.TryGetValue(key, out set))
                {
                    return;
                }
                set.Remove(order);
                cancelList.Remove(order);
                if (set.Count == 0)
                {
                    Grid.Remove(key);
                }
            }
        }

        public double Size(HashSet<Order> set)
        {
            lock (this)
            {
                double sum = 0;
                foreach (var o in set)
                {
                    sum += o.LeavesQty;
                }
                return sum;
            }
        }

        public double Size()
        {
            lock (this)
            {
                double sum = 0;
                foreach (HashSet<Order> set in Grid.Values)
                {
                    sum += Size(set);
                }
                return sum;
            }
        }

        public double SizeByIndex(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= Grid.Count)
                    return 0;

                HashSet<Order> set = Grid.Values[index];
                return Size(set);
            }
        }

        public double SizeByLevel(int level)
        {
            lock (this)
            {
                HashSet<Order> set;
                if (!Grid.TryGetValue(level, out set))
                {
                    return 0;
                }
                return Size(set);
            }
        }

        public double SizeByPrice(double price)
        {
            lock (this)
            {
                int key = PriceHelper.GetLevelByPrice(price, Side);
                return SizeByLevel(key);
            }            
        }

        public int Cancel()
        {
            lock (this)
            {
                int cnt = 0;
                foreach (var set in Grid.Values)
                {
                    cnt += Cancel(set);
                }
                return cnt;
            }
        }

        public int Cancel(HashSet<Order> set)
        {
            lock (this)
            {
                int cnt = 0;
                foreach (var o in set)
                {
                    if (!o.IsDone)
                    {
                        o.Cancel();
                        cancelList.Add(o);
                        ++cnt;
                    }
                }
                return cnt;
            }
        }

        public int CancelByIndex(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= Grid.Count)
                    return 0;

                HashSet<Order> set = Grid.Values[index];
                return Cancel(set);
            }
        }

        public int CancelNotEqualPrice(double price)
        {
            lock (this)
            {
                int cnt = 0;
                int key = PriceHelper.GetLevelByPrice(price, Side);
                foreach (var kv in Grid)
                {
                    if (kv.Key != key)
                    {
                        cnt += Cancel(kv.Value);
                    }
                }
                return cnt;
            }
        }

        public double PriceByIndex(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= Grid.Count)
                    return 0;

                int key = Grid.Keys[index];
                return PriceHelper.GetPriceByLevel(key);
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (var i in Grid)
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

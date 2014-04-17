using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class OrderBook_OneSide_Order : IComparer<int>, IComparer<Order>
    {
        public OrderSide Side;
        private SortedList<int, SortedSet<Order>> _Grid;
        public PriceHelper PriceHelper;
        private HashSet<Order> cancelList = new HashSet<Order>();

        public IEnumerable<KeyValuePair<int, SortedSet<Order>>> GridList
        {
            get
            {
                lock (this)
                {
                    return _Grid.ToList();
                }
            }
        }

        public OrderBook_OneSide_Order(OrderSide Side)
        {
            this.Side = Side;
            _Grid = new SortedList<int, SortedSet<Order>>(this);
        }

        public int Compare(int x, int y)
        {
            return x.CompareTo(y) * (Side == OrderSide.Buy ? -1 : 1);
        }

        public int Compare(Order x, Order y)
        {
            return x.DateTime.CompareTo(y.DateTime);
        }

        public bool IsPending
        {
            get { return _Grid.Count > 0; }
        }

        public bool IsCancelling
        {
            get { return cancelList.Count > 0; }
        }

        public void Clear()
        {
            lock(this)
            {
                _Grid.Clear();
            }
        }

        public int Count
        {
            get { return _Grid.Count; }
        }


        public void Add(Order order)
        {
            lock(this)
            {
                SortedSet<Order> set;
                int key = PriceHelper.GetLevelByPrice(order.Price, Side);
                if (!_Grid.TryGetValue(key, out set))
                {
                    set = new SortedSet<Order>(this);
                    _Grid.Add(key, set);
                }
                set.Add(order);
            }
        }

        public void Remove(Order order)
        {
            lock (this)
            {
                cancelList.Remove(order);

                SortedSet<Order> set;
                int key = PriceHelper.GetLevelByPrice(order.Price, Side);
                if (!_Grid.TryGetValue(key, out set))
                {
                    return;
                }
                set.Remove(order);
                if (set.Count == 0)
                {
                    _Grid.Remove(key);
                }
            }
        }

        public double Size(SortedSet<Order> set)
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
                foreach (SortedSet<Order> set in _Grid.Values)
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
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                SortedSet<Order> set = _Grid.Values[index];
                return Size(set);
            }
        }

        public double SizeByLevel(int level)
        {
            lock (this)
            {
                SortedSet<Order> set;
                if (!_Grid.TryGetValue(level, out set))
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
                foreach (var set in _Grid.Values.ToList())
                {
                    cnt += Cancel(set);
                }
                return cnt;
            }
        }

        public int Cancel(SortedSet<Order> set)
        {
            lock (this)
            {
                int cnt = 0;
                foreach (var o in set.ToList())
                {
                    if (!o.IsDone)
                    {
                        cancelList.Add(o);
                        o.Cancel();
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
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                SortedSet<Order> set = _Grid.Values[index];
                return Cancel(set);
            }
        }

        //public int CancelEqualTopThanLevel(int level)
        //{
        //    lock (this)
        //    {
        //        int cnt = 0;
        //        foreach (var kv in _Grid.ToList())
        //        {
        //            if (Side == OrderSide.Buy)
        //            {
        //                if (kv.Key <= level)
        //                {
        //                    cnt += Cancel(kv.Value);
        //                }
        //            }
        //            else
        //            {
        //                if (kv.Key >= level)
        //                {
        //                    cnt += Cancel(kv.Value);
        //                }
        //            }
        //        }
        //        return cnt;
        //    }
        //}

        public int CancelNotEqualPrice(double price)
        {
            lock (this)
            {
                int cnt = 0;
                int key = PriceHelper.GetLevelByPrice(price, Side);
                foreach (var kv in _Grid)
                {
                    if (kv.Key != key)
                    {
                        cnt += Cancel(kv.Value);
                    }
                }
                return cnt;
            }
        }

        //public int CancelFarFromPrice(double price)
        //{
        //    lock (this)
        //    {
        //        //int cnt = 0;
        //        //int key = PriceHelper.GetLevelByPrice(price, Side);
        //        //foreach (var kv in Grid)
        //        //{
        //        //    if (kv.Key != key)
        //        //    {
        //        //        cnt += Cancel(kv.Value);
        //        //    }
        //        //}
        //        return cnt;
        //    }
        //}

        public double PriceByIndex(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                int key = _Grid.Keys[index];
                return PriceHelper.GetPriceByLevel(key);
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (var i in _Grid)
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

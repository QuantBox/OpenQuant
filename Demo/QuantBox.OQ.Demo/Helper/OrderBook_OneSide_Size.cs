using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 单向挂单管理
    /// </summary>
    public class OrderBook_OneSide_Size : IComparer<int>
    {
        public OrderSide Side;
        private SortedList<int, double> _Grid;
        public PriceHelper PriceHelper;

        public IEnumerable<KeyValuePair<int, double>> GridList
        {
            get
            {
                lock (this)
                {
                    return _Grid.ToList();
                }
            }
        }

        public OrderBook_OneSide_Size(OrderSide Side)
        {
            this.Side = Side;
            _Grid = new SortedList<int, double>(this);
        }

        public int Compare(int x, int y)
        {
            return x.CompareTo(y) * (Side == OrderSide.Buy ? -1 : 1);
        }

        public void Change(double price, double size)
        {
            lock(this)
            {
                int key = PriceHelper.GetLevelByPrice(price, Side);
                double value;
                if(_Grid.TryGetValue(key,out value))
                {
                    if (value + size < 0)
                    {
                        _Grid.Remove(key);
                    }
                    else
                    {
                        _Grid[key] = value + size;
                    }
                    
                }
            }
        }

        public void Set(double price, double size)
        {
            lock(this)
            {
                int key = PriceHelper.GetLevelByPrice(price, Side);
                if (size <= 0)
                {
                    _Grid.Remove(key);
                }
                else
                {
                    _Grid[key] = size;
                }
            }
        }

        public void SetOnly(double price, double size)
        {
            lock(this)
            {
                Clear();
                Set(price, size);
            }
        }

        public double SizeByLevel(int level)
        {
            lock (this)
            {
                double size;
                if (!_Grid.TryGetValue(level, out size))
                {
                    return 0;
                }
                return size;
            }
        }

        public int LevelByIndex(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                return _Grid.Keys[index];
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _Grid.Clear();
            }
        }

        public int Count
        {
            get { return _Grid.Count; }
        }
    }
}

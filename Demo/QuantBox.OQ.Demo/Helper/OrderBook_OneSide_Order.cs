using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 挂单方向及动作
    /// </summary>
    public class OrderBook_OneSide_Order : IComparer<int>, IComparer<Order>
    {
        /// <summary>
        /// //订单方向（buy or sell）
        /// </summary>
        public OrderSide Side;
        /// <summary>
        /// 订单的排序列表的集合
        /// </summary>
        private SortedList<int, SortedSet<Order>> _Grid;
        /// <summary>
        /// //价格助手
        /// </summary>
        public PriceHelper PriceHelper;
        /// <summary>
        /// //撤单的散列集合
        /// </summary>
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
        /// <summary>
        /// 比较两个订单的时间先后顺序
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(Order x, Order y)
        {
            return x.DateTime.CompareTo(y.DateTime);
        }
        /// <summary>
        /// 订单数>0时，需要等待
        /// </summary>
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
        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="order"></param>
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
        /// <summary>
        /// 到剩余的订单数量之和
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public double Size(SortedSet<Order> set)
        {
            //lock (this)
            {
                double sum = 0;
                foreach (var o in set)
                {
                    sum += o.LeavesQty;//得到剩余的订单数量之和
                }
                return sum;
            }
        }
        /// <summary>
        /// 获取所有订单的
        /// </summary>
        /// <returns></returns>
        public double Size()
        {
            //lock (this)
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
            //lock (this)
            {
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                SortedSet<Order> set = _Grid.Values[index];
                return Size(set);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public double SizeByLevel(int level)
        {
            //lock (this)
            {
                SortedSet<Order> set;
                if (!_Grid.TryGetValue(level, out set))
                {
                    return 0;
                }
                return Size(set);
            }
        }
        /// <summary>
        /// 获取价格水平，取TickSize整数倍
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public double SizeByPrice(double price)
        {
            lock (this)
            {
                int key = PriceHelper.GetLevelByPrice(price, Side);
                return SizeByLevel(key);
            }            
        }
        /// <summary>
        /// 撤单
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 撤消挂单
        /// </summary>
        /// <param name="set">订单集合</param>
        /// <returns>返回撤单数量</returns>
        public int Cancel(SortedSet<Order> set)
        {
            lock (this)
            {
                int cnt = 0;//撤单数量
                foreach (var o in set.ToList())// 0 为推断类型变量，他的类型取决存储在set中的类型
                {
                    if (!o.IsDone)//返回true,如果这个订单最终状态(填充,拒绝或取消)
                    {
                        cancelList.Add(o);//撤单列表添加
                        o.Cancel();//取消此订单
                        ++cnt;
                    }
                }
                return cnt;
            }
        }
        /// <summary>
        /// 撤消对应挂单
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 取消与price不同的
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public int CancelNotEqualPrice(double price)
        {
            lock (this)
            {
                int cnt = 0;
                int key = PriceHelper.GetLevelByPrice(price, Side);
                foreach (var kv in _Grid.ToList())
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
        /// <summary>
        /// 价格索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double PriceByIndex(int index)
        {
            //lock (this)
            {
                if (index < 0 || index >= _Grid.Count)
                    return 0;

                int key = _Grid.Keys[index];
                return PriceHelper.GetPriceByLevel(key);
            }
        }
        /// <summary>
        /// 显示对应订单价格、剩余订单数量和
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            foreach (var i in _Grid)
            {
                double price = PriceHelper.GetPriceByLevel(i.Key);//获得订单价格
                double sum = 0;//剩余的订单总数量
                foreach (var o in i.Value)
                {
                    sum += o.LeavesQty;//LeavesQty得到剩余的订单数量
                }
                str += string.Format("{0} {1}{2}", price, sum, Environment.NewLine);//Environment.NewLine转换字符串
            }
            return str;
        }
    }
}

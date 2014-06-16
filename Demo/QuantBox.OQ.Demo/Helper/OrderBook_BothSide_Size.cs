using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    /// <summary>
    /// 双向挂单管理
    /// </summary>
    public class OrderBook_BothSide_Size
    {
        public OrderBook_OneSide_Size Sell = new OrderBook_OneSide_Size(OrderSide.Sell);
        public OrderBook_OneSide_Size Buy = new OrderBook_OneSide_Size(OrderSide.Buy);

        public void Clear()
        {
            Sell.Clear();
            Buy.Clear();
        }
    }
}

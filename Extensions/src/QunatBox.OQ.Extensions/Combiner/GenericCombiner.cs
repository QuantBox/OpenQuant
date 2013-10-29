using QuantBox.OQ.Extensions.Combiner;
using QuantBox.OQ.Extensions.OrderItem;
using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class GenericCombiner<T,U>
        where T : GenericOrderItem,new()
        where U : TextCommon
    {
        private T item = null;

        public T Add(SingleOrder order, U u)
        {
            if (item == null || item.IsCreated())
            {
                item = new T();
                item.Init(u);
            }

            item.Add(order, u);

            if (item.IsCreated())
            {
                return item;
            }

            return null;
        }
    }
}

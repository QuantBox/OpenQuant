using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class GenericCombiner<T,U>
        where T : GenericOrderItem,new()
        where U : TextRequest
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

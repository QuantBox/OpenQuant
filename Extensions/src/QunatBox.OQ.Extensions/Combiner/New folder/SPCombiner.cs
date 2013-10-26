using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class SPCombiner
    {
        private SPOrderItem item = null;

        public SPOrderItem Add(SingleOrder order, TextSP t)
        {
            if (item == null || item.IsDone())
            {
                item = new SPOrderItem(t);
            }

            item.Add(order, t);

            if (item.IsDone())
            {
                return item;
            }

            return null;
        }
    }
}

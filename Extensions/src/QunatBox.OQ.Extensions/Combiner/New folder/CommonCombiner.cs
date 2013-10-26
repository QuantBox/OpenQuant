using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class CommonCombiner
    {
        private CommonOrderItem item = null;

        public CommonOrderItem Add(SingleOrder order, TextRequest t)
        {
            if(item == null || item.IsDone())
            {
                item = new CommonOrderItem();
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

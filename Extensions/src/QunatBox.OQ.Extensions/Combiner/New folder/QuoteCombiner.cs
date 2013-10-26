using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class QuoteCombiner
    {
        private QuoteOrderItem item = null;

        public QuoteOrderItem Add(SingleOrder order, TextQuote t)
        {
            if(item == null || item.IsDone())
            {
                item = new QuoteOrderItem(t);
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

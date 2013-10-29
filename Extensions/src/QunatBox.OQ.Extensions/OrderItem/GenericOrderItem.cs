using QuantBox.OQ.Extensions.Combiner;
using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.OrderItem
{
    public class GenericOrderItem
    {
        public TextCommon TextRequest;

        public bool IsPendingCancel;

        public virtual void Init(TextCommon t)
        {
            TextRequest = t;
        }

        public virtual void Add(SingleOrder order, TextCommon t)
        {
        }

        public virtual IEnumerable<MultiOrderLeg> GetLegs()
        {
            return null;
        }

        public virtual MultiOrderLeg GetLeg(Side side, string instrument)
        {
            return null;
        }

        public virtual int GetLegNum()
        {
            return 1;
        }

        public virtual bool IsCreated()
        {
            return false;
        }

        public virtual bool IsDone()
        {
            foreach (var order in GetLegs())
            {
                if (order.Order.IsDone)
                    return true;
            }
            return false;
        }
    }
}

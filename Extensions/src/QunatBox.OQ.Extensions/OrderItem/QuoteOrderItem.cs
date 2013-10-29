using QuantBox.OQ.Extensions.Combiner;
using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.OrderItem
{
    public class QuoteOrderItem : GenericOrderItem
    {
        public MultiOrderLeg Sell;
        public MultiOrderLeg Buy;

        public string QuoteID;
        public int StayTime;

        public override void Init(TextCommon t)
        {
            base.Init(t);

            TextQuote q = t as TextQuote;
            QuoteID = q.QuoteID;
            StayTime = q.StayTime;
        }

        public override void Add(SingleOrder order, TextCommon t)
        {
            if (order.Side == Side.Buy)
            {
                Buy = new MultiOrderLeg() { Order = order, OpenClose = t.OpenClose };
            }
            else
            {
                Sell = new MultiOrderLeg() { Order = order, OpenClose = t.OpenClose };
            }
        }

        public override IEnumerable<MultiOrderLeg> GetLegs()
        {
            return new List<MultiOrderLeg>() { Buy, Sell};
        }

        public override MultiOrderLeg GetLeg(Side side,string instrument)
        {
            if (side == Side.Buy)
                return Buy;
            else
                return Sell;
        }

        public override int GetLegNum()
        {
            return 2;
        }

        public override bool IsCreated()
        {
            if (string.IsNullOrEmpty(QuoteID))
                return false;

            if (Sell == null || Buy == null)
                return false;

            return true;
        }
    }
}

using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System.Collections.Generic;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class SPOrderItem : GenericOrderItem
    {
        public int Index;
        public MultiOrderLeg[] Leg = new MultiOrderLeg[2];

        public override void Init(TextCommon t)
        {
            base.Init(t);
            Index = 0;
        }

        public override void Add(SingleOrder order, TextCommon t)
        {
            if (Index >= GetLegNum())
                return;

            Leg[Index] = new MultiOrderLeg { Order = order, OpenClose = t.OpenClose };
            ++Index;
        }

        public override IEnumerable<MultiOrderLeg> GetLegs()
        {
            return new List<MultiOrderLeg>() { Leg[0], Leg[1]};
        }

        public override MultiOrderLeg GetLeg(Side side, string instrument)
        {
            foreach (var l in Leg)
            {
                if(l.Order.Side == side)
                {
                    return l;
                }
            }
            return null;
        }

        public override int GetLegNum()
        {
            return 2;
        }

        public override bool IsCreated()
        {
            return Index == GetLegNum();
        }

        public string GetSymbol()
        {
            if (!IsCreated())
                return null;

            return string.Format("{0} {1}/{2}",TextRequest.Type,Leg[0].Order.Symbol, Leg[1].Order.Symbol);
        }
    }
}

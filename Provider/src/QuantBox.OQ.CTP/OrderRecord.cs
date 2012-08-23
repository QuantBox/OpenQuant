using SmartQuant.Execution;

namespace QuantBox.OQ.CTP
{
    class OrderRecord
    {
        // Fields
        private double avgPx;
        private int cumQty;
        private int leavesQty;
        private SingleOrder order;

        // Methods
        public OrderRecord(SingleOrder order)
        {
            this.order = order;
            this.avgPx = 0.0;
            this.leavesQty = (int)order.OrderQty;
            this.cumQty = 0;
        }

        public void AddFill(double lastPx, int lastQty)
        {
            this.avgPx = ((this.avgPx * this.cumQty) + (lastPx * lastQty)) / ((double)(this.cumQty + lastQty));
            this.leavesQty -= lastQty;
            this.cumQty += lastQty;
        }

        // Properties
        public double AvgPx
        {
            get
            {
                return this.avgPx;
            }
        }

        public int CumQty
        {
            get
            {
                return this.cumQty;
            }
        }

        public int LeavesQty
        {
            get
            {
                return this.leavesQty;
            }
        }

        public SingleOrder Order
        {
            get
            {
                return this.order;
            }
        }
    }

}

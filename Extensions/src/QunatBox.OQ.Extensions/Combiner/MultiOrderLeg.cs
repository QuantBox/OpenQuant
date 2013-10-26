using SmartQuant.Execution;

namespace QuantBox.OQ.Extensions.Combiner
{
    /// <summary>
    /// 组合报单中的单腿
    /// </summary>
    public class MultiOrderLeg
    {
        public SingleOrder Order;
        public EnumOpenClose OpenClose;
    }
}

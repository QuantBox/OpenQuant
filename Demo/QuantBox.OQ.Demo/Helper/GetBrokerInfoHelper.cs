using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class GetBrokerInfoHelper
    {
        public const string PosiDirection = "PosiDirection";
        public const string Long = "Long";
        public const string PositionDate = "PositionDate";
        public const string Today = "Today";
        public const string Position = "Position";

        public static void Transform(BrokerAccount brokerAccount,DualPosition dualPosition)
        {
            if (brokerAccount == null||dualPosition == null)
                return;

            int LongQtyYd = 0;
            int LongQtyToday = 0;
            int ShortQtyYd = 0;
            int ShortQtyToday = 0;

            foreach (BrokerPosition bp in brokerAccount.Positions)
            {
                if (bp.Symbol != dualPosition.Symbol)
                    continue;

                // 会收到很多，得按类别进行设置
                // 按日子、多空
                // 投机与套保这种不进行区分，因为太复杂了，我也没法测试
                if (bp.Fields[PosiDirection].Value == Long)
                {
                    if (bp.Fields[PositionDate].Value == Today)
                    {
                        LongQtyToday += Convert.ToInt32(bp.Fields[Position].Value);
                    }
                    else
                    {
                        LongQtyYd += Convert.ToInt32(bp.Fields[Position].Value);
                    }
                }
                else
                {
                    if (bp.Fields[PositionDate].Value == Today)
                    {
                        ShortQtyToday += Convert.ToInt32(bp.Fields[Position].Value);
                    }
                    else
                    {
                        ShortQtyYd += Convert.ToInt32(bp.Fields[Position].Value);
                    }
                }
            }

            dualPosition.Long.QtyToday = LongQtyToday;
            dualPosition.Long.Qty = LongQtyToday + LongQtyYd;
            dualPosition.Short.QtyToday = ShortQtyToday;
            dualPosition.Short.Qty = ShortQtyToday + ShortQtyYd;
        }
    }
}

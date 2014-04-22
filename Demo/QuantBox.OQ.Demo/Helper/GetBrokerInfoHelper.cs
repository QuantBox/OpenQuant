using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public class GetBrokerInfoHelper
    {
        private const string PosiDirection = "PosiDirection";
        private const string Long = "Long";
        private const string PositionDate = "PositionDate";
        private const string Today = "Today";
        private const string Position = "Position";

        public static void Transform(BrokerAccount brokerAccount,DualPosition dualPosition)
        {
            if (brokerAccount == null||dualPosition == null)
                return;

            int LongQtyToday = 0;
            int LongQtyYd = 0;
            int ShortQtyToday = 0;
            int ShortQtyYd = 0;

            Tarnsform(brokerAccount, dualPosition.Symbol,
                out LongQtyToday, out LongQtyYd,
                out ShortQtyToday,out ShortQtyYd);

            dualPosition.Long.QtyToday = LongQtyToday;
            dualPosition.Long.Qty = LongQtyToday + LongQtyYd;
            dualPosition.Short.QtyToday = ShortQtyToday;
            dualPosition.Short.Qty = ShortQtyToday + ShortQtyYd;
        }

        public static void Tarnsform(BrokerAccount brokerAccount,string Symbol,
            out int LongQtyToday,
            out int LongQtyYd,
            out int ShortQtyToday,
            out int ShortQtyYd
            )
        {
            LongQtyToday = 0;
            LongQtyYd = 0;
            ShortQtyToday = 0;
            ShortQtyYd = 0;

            foreach (BrokerPosition bp in brokerAccount.Positions)
            {
                if (bp.Symbol != Symbol)
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
        }
    }
}

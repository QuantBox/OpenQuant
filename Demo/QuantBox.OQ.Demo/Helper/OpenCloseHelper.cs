using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public enum EnumOpenClose
    {
        OPEN,
        CLOSE,
    }

    public class OpenCloseHelper
    {
        public const string OPEN_PREFIX = "O|";
        public const string CLOSE_TODAY_PREFIX = "T|";
        public const string CLOSE_YESTODAY_PREFIX = "Y|";
        public const string CLOSE_PREFIX = "C|";


        public static EnumOpenClose CheckOpenClose(Order order)
        {
            return order.Text.StartsWith(OPEN_PREFIX)?EnumOpenClose.OPEN:EnumOpenClose.CLOSE;
        }

        public static string GetOpenCloseString(EnumOpenClose oc)
        {
            switch (oc)
            {
                case EnumOpenClose.OPEN:
                    return OPEN_PREFIX;
                case EnumOpenClose.CLOSE:
                    return CLOSE_TODAY_PREFIX;
                default:
                    return OPEN_PREFIX;
            }
        }


        public static PositionSide CheckLongShort(Order order)
        {
            if (EnumOpenClose.OPEN == CheckOpenClose(order))
            {
                if (order.Side == OrderSide.Buy)
                {

                    return PositionSide.Long;
                }
            }
            else
            {
                if (order.Side == OrderSide.Buy)
                {
                }
                else
                {
                    return PositionSide.Long;
                }
            }
            return PositionSide.Short;
        }
    }
}

using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public enum EnumTradingTime
    {
        FINANCIAL,
        COMMODITY,
        AUAG,
    }

    public class TimeHelper
    {
        public int[] WorkingTime;
        public int EndOfDay;

        public int[] WorkingTime_Financial = { 915, 1130, 1300, 1515 }; //IF
        public int[] WorkingTime_Commodity = { 900, 1015, 1030, 1130, 1330, 1500 }; //商品
        public int[] WorkingTime_AuAg = { 0, 230, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//au,ag

        public int EndOfDay_Financial = 1515; //IF
        public int EndOfDay_Commodity = 1500; //商品

        public TimeHelper(EnumTradingTime tt)
        {
            switch(tt)
            {
                case EnumTradingTime.FINANCIAL:
                    WorkingTime = WorkingTime_Financial;
                    EndOfDay = EndOfDay_Financial;
                    break;
                case EnumTradingTime.COMMODITY:
                    WorkingTime = WorkingTime_Commodity;
                    EndOfDay = EndOfDay_Commodity;
                    break;
                case EnumTradingTime.AUAG:
                    WorkingTime = WorkingTime_AuAg;
                    EndOfDay = EndOfDay_Commodity;
                    break;
            }
        }

        public bool IsTradingTime(int time,int nOffset)
        {
            int index = -1;
            for (int i = 0; i < WorkingTime.Length; ++i)
            {
                if (time < WorkingTime[i] + (i % 2 == 0 ? 0 : nOffset))
                {
                    break;
                }
                else
                {
                    index = i;
                }
            }

            if (index % 2 == 0)
            {
                // 交易时段
                return true;
            }

            // 非交易时段
            return false;
        }

        public bool IsTradingTime(DateTime dt, int nOffset)
        {
            int time = dt.Hour * 100 + dt.Minute;

            return IsTradingTime(time, nOffset);
        }

        public bool IsTradingTime(int nOffset)
        {
            return IsTradingTime(Clock.Now, nOffset);
        }

        public bool IsTradingTime()
        {
            return IsTradingTime(0);
        }

        public int GetEndOfDay()
        {
            return EndOfDay;
        }
    }
}

using OpenQuant.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Demo.Helper
{
    public enum EnumTradingTime
    {
        FINANCIAL,  // 金融
        COMMODITY,  // 商品
        COMMODITY_0230, // 黄金、白银
        COMMODITY_0100,
    }
    /// <summary>
    /// 时间助手
    /// </summary>
    public class TimeHelper
    {
        /// <summary>
        /// 交易时间
        /// </summary>
        public int[] WorkingTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public int EndOfDay { get; private set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public int BeginOfDay { get; private set; }

        public int[] WorkingTime_Financial = { 915, 1130, 1300, 1515 }; //IF,TF,IO
        public int[] WorkingTime_Commodity = { 900, 1015, 1030, 1130, 1330, 1500 }; //商品
        public int[] WorkingTime_Commodity_0230 = { 0, 230, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//au,ag
        public int[] WorkingTime_Commodity_0100 = { 0, 100, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//铜、铝、铅、锌

        private int EndOfDay_Financial = 1515; //IF 结束时间
        private int EndOfDay_Commodity = 1500; //商品结束时间

        private int BeginOfDay_Financial = 915;//上期所的开始时间
        private int BeginOfDay_Commodity = 900;//商品的开始时间
        private int BeginOfDay_Commodity_0230 = 2100;
        private int BeginOfDay_Commodity_0100 = 2100;
        /// <summary>
        /// 时间助手，根据所属交易所的类型确定交易时间
        /// </summary>
        /// <param name="tt"></param>
        public TimeHelper(EnumTradingTime tt)
        {
            switch(tt)
            {
                case EnumTradingTime.FINANCIAL:
                    WorkingTime = WorkingTime_Financial;
                    EndOfDay = EndOfDay_Financial;
                    BeginOfDay = BeginOfDay_Financial;
                    break;
                case EnumTradingTime.COMMODITY:
                    WorkingTime = WorkingTime_Commodity;
                    EndOfDay = EndOfDay_Commodity;
                    BeginOfDay = BeginOfDay_Commodity;
                    break;
                case EnumTradingTime.COMMODITY_0230:
                    WorkingTime = WorkingTime_Commodity_0230;
                    EndOfDay = EndOfDay_Commodity;
                    BeginOfDay = BeginOfDay_Commodity_0230;
                    break;
                case EnumTradingTime.COMMODITY_0100:
                    WorkingTime = WorkingTime_Commodity_0100;
                    EndOfDay = EndOfDay_Commodity;
                    BeginOfDay = BeginOfDay_Commodity_0100;
                    break;
            }
        }

        public TimeHelper(string instrument):this(GetTradingTime(instrument))
        {
        }
        /// <summary>
        /// 时间助手构造函数
        /// </summary>
        /// <param name="workingTime">交易时间</param>
        /// <param name="beginOfDay">开始时间</param>
        /// <param name="ennOfDay">结束时间</param>
        public TimeHelper(int[] workingTime,int beginOfDay,int ennOfDay)
        {
            WorkingTime = workingTime;
            BeginOfDay = beginOfDay;
            EndOfDay = ennOfDay;
        }
        /// <summary>
        /// 合约号前缀获取交易所时间
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        public static EnumTradingTime GetTradingTime(string instrument)
        {
            string prefix = instrument.Substring(0, 2);//得到前缀2个字符
            switch(prefix)
            {
                case "IF":
                case "TF":
                case "IO":
                case "IH":
                case "IC":
                case "HO":
                    return EnumTradingTime.FINANCIAL;
                case "au":
                case "ag":
                    return EnumTradingTime.COMMODITY_0230;
                case "cu":
                case "al":
                case "pb":
                case "zn":
                    return EnumTradingTime.COMMODITY_0100;
                default:
                    return EnumTradingTime.COMMODITY;
            }
        }
        /// <summary>
        /// 时否交易时间
        /// </summary>
        /// <param name="time">整型参数</param>
        /// <returns></returns>
        public bool IsTradingTime(int time)
        {
            int index = -1;
            for (int i = 0; i < WorkingTime.Length; ++i)
            {
                if (time < WorkingTime[i])
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
        /// <summary>
        /// 得到数字表示的时间，形如9:15表示为915
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int GetTime(DateTime dt)
        {
            return dt.Hour * 100 + dt.Minute;
        }
        /// <summary>
        /// 时否交易时间
        /// </summary>
        /// <param name="dt">时间参数</param>
        /// <returns></returns>
        public bool IsTradingTime(DateTime dt)
        {
            return IsTradingTime(GetTime(dt));
        }
        /// <summary>
        /// 是否交易时间
        /// </summary>
        /// <returns></returns>
        public bool IsTradingTime()
        {
            return IsTradingTime(Clock.Now);
        }
    }
}

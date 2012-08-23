using System;
using System.Data;
using System.Linq;
using QuantBox.CSharp2C;

namespace QuantBox.OQ.CTP
{
    class DbInMemInvestorPosition
    {
        public const string InstrumentID = "InstrumentID";
        public const string PosiDirection = "PosiDirection";
        public const string HedgeFlag = "HedgeFlag";
        public const string PositionDate = "PositionDate";
        public const string Position = "Position";

        private DataTable dtInvestorPosition = new DataTable("Position");

        public DbInMemInvestorPosition()
        {
            dtInvestorPosition.Columns.Add(InstrumentID, Type.GetType("System.String"));
            dtInvestorPosition.Columns.Add(PosiDirection, typeof(QuantBox.CSharp2C.TThostFtdcPosiDirectionType));
            dtInvestorPosition.Columns.Add(HedgeFlag, typeof(QuantBox.CSharp2C.TThostFtdcHedgeFlagType));
            dtInvestorPosition.Columns.Add(PositionDate, typeof(QuantBox.CSharp2C.TThostFtdcPositionDateType));
            dtInvestorPosition.Columns.Add(Position, Type.GetType("System.Int32"));
            //dtInvestorPosition.Columns.Add("TodayPosition", Type.GetType("System.Int32"));
            //因为PositionDate有了区分，所以TodayPosition可以不专门用字段记录

            UniqueConstraint uniqueConstraint = new UniqueConstraint(new DataColumn[] {
                        dtInvestorPosition.Columns[InstrumentID],
                        dtInvestorPosition.Columns[PosiDirection],
                        dtInvestorPosition.Columns[HedgeFlag],
                        dtInvestorPosition.Columns[PositionDate]
                    });
            dtInvestorPosition.Constraints.Add(uniqueConstraint);
        }

        //private int x = 0;

        //查询持仓后调用此函数
        public bool InsertOrReplace(
            string InstrumentID,
            TThostFtdcPosiDirectionType PosiDirection,
            TThostFtdcHedgeFlagType HedgeFlag,
            TThostFtdcPositionDateType PositionDate,
            int volume)
        {
            lock(this)
            {
                //冲突的可能性大一些，所以要先Update后Insert
                DataRow[] rows = Select(InstrumentID,
                    PosiDirection,
                    HedgeFlag,
                    PositionDate);

                if (rows.Count() == 1)
                {
                    rows[0][Position] = volume;
                }
                else
                {
                    try
                    {
                        dtInvestorPosition.Rows.Add(
                            InstrumentID,
                            PosiDirection,
                            HedgeFlag,
                            PositionDate,
                            volume);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //开仓时调用
        public bool InsertOrReplaceForOpen(
            string InstrumentID,
            TThostFtdcPosiDirectionType PosiDirection,
            TThostFtdcHedgeFlagType HedgeFlag,
            TThostFtdcPositionDateType PositionDate,
            int volume)
        {
            lock(this)
            {
                //冲突的可能性大一些，所以要先Update后Insert
                DataRow[] rows = Select(InstrumentID, PosiDirection, HedgeFlag, PositionDate);

                if (rows.Count() == 1)
                {
                    rows[0][Position] = volume + (int)rows[0][Position];
                }
                else
                {
                    try
                    {
                        dtInvestorPosition.Rows.Add(
                                        InstrumentID,
                                        PosiDirection,
                                        HedgeFlag,
                                        PositionDate,
                                        volume);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //只有在平今或平昨时才会调用
        public int ReplaceForClose(
            string InstrumentID,
            TThostFtdcPosiDirectionType PosiDirection,
            TThostFtdcHedgeFlagType HedgeFlag,
            TThostFtdcPositionDateType PositionDate,
            int volume)
        {
            lock(this)
            {
                //冲突的可能性大一些，所以要先Update后Insert
                DataRow[] rows = Select(InstrumentID, PosiDirection, HedgeFlag, PositionDate);

                int restVol = volume;
                foreach (DataRow dr in rows)
                {
                    if (restVol > 0)
                    {
                        int vol = (int)dr[Position];
                        if (restVol - vol >= 0)
                        {
                            //不够，得多设置几条
                            dr[Position] = 0;
                            restVol -= vol;
                        }
                        else
                        {
                            //足够了，只设置当前即可
                            dr[Position] = vol - restVol;
                            restVol = 0;
                        }
                    }
                }
                return restVol;
            }
        }

        //只有在TThostFtdcOffsetFlagType.Close时才调用这一函数
        public int ReplaceForClose(
            string InstrumentID,
            TThostFtdcPosiDirectionType PosiDirection,
            TThostFtdcHedgeFlagType HedgeFlag,
            int volume)
        {
            lock(this)
            {
                DataView view = dtInvestorPosition.DefaultView;
                view.RowFilter = string.Format("InstrumentID='{0}' and PosiDirection={1} and HedgeFlag={2}",
                            InstrumentID,
                            (int)PosiDirection,
                            (int)HedgeFlag);
                view.Sort = string.Format("PositionDate DESC");//将历史的排前面


                int restVol = volume;
                foreach (DataRowView dr in view)
                {
                    if (restVol > 0)
                    {
                        int vol = (int)dr[Position];
                        if (restVol - vol >= 0)
                        {
                            //不够，得多设置几条
                            dr[Position] = 0;
                            restVol -= vol;
                        }
                        else
                        {
                            //足够了，只设置当前即可
                            dr[Position] = vol - restVol;
                            restVol = 0;
                        }
                    }
                }

                return restVol;
            }
        }

        public void GetPositions(
            string InstrumentID,
            TThostFtdcPosiDirectionType PosiDirection,
            TThostFtdcHedgeFlagType HedgeFlag,
            out int YdPosition,
            out int TodayPosition)
        {
            YdPosition = 0;
            TodayPosition = 0;
            DataView view = dtInvestorPosition.DefaultView;
            view.RowFilter = string.Format("InstrumentID='{0}' and PosiDirection={1} and HedgeFlag={2}",
                        InstrumentID,
                        (int)PosiDirection,
                        (int)HedgeFlag);

            foreach (DataRowView dr in view)
            {
                int vol = (int)dr[Position];
                TThostFtdcPositionDateType PositionDate1 = (TThostFtdcPositionDateType)dr[PositionDate];
                if (TThostFtdcPositionDateType.Today == PositionDate1)
                {
                    TodayPosition += vol;
                }
                else
                {
                    YdPosition += vol;
                }
            }
        }

        public DataRow[] Select(string InstrumentID, TThostFtdcPosiDirectionType PosiDirection, TThostFtdcHedgeFlagType HedgeFlag, TThostFtdcPositionDateType PositionDate)
        {
            return dtInvestorPosition.Select(
                string.Format("InstrumentID='{0}' and PosiDirection={1} and HedgeFlag={2} and PositionDate={3}",
                        InstrumentID,
                        (int)PosiDirection,
                        (int)HedgeFlag,
                        (int)PositionDate));
        }

        public DataRow[] SelectAll()
        {
            return dtInvestorPosition.Select();
        }

        public bool UpdateByTrade(CThostFtdcTradeField pTrade)
        {
            /*
             * Q:向非上海市场发送平今指今，系统能自动处理成平今，接到的回报是平今还是平仓?
             * A:上海市场的昨仓用Close和CloseYesterday都能平，报单回报不会改变，而成交回报会修改成CloseYesterday
             * 
             * Q:是否不同市场收到的成交回报都分为了今天与历史。非上海市场不时用Close时，返回数据能区分CloseToday和CloseYesterday吗？还是只回Close
             * 
             * Q:上海强平今仓与昨仓会收到什么结果？
             * 
             * Q:非上海市场，一手今，一手昨，同时平，收到的回报会如何？
             * 
             */

            //如果是开仓，查找唯一的那条记录，不存在就插入
            //如果是平今，只查找今天的记录，直接修改
            //如果是平昨，只处理昨天
            //如果是平仓，从历史开始处理
            lock(this)
            {
                TThostFtdcPosiDirectionType PosiDirection = TThostFtdcPosiDirectionType.Short;
                TThostFtdcPositionDateType PositionDate = TThostFtdcPositionDateType.Today;
                if (TThostFtdcOffsetFlagType.Open == pTrade.OffsetFlag)
                {
                    if (TThostFtdcDirectionType.Buy == pTrade.Direction)
                    {
                        PosiDirection = TThostFtdcPosiDirectionType.Long;
                    }
                    return InsertOrReplaceForOpen(pTrade.InstrumentID,
                        PosiDirection,
                        pTrade.HedgeFlag,
                        PositionDate,
                        pTrade.Volume);
                }
                else
                {
                    if (TThostFtdcDirectionType.Sell == pTrade.Direction)
                    {
                        PosiDirection = TThostFtdcPosiDirectionType.Long;
                    }

                    if (TThostFtdcOffsetFlagType.CloseToday == pTrade.OffsetFlag)
                    {
                        return ReplaceForClose(pTrade.InstrumentID,
                            PosiDirection,
                            pTrade.HedgeFlag,
                            PositionDate,
                            pTrade.Volume) == 0;
                    }
                    else if (TThostFtdcOffsetFlagType.CloseYesterday == pTrade.OffsetFlag)
                    {
                        PositionDate = TThostFtdcPositionDateType.History;
                        return ReplaceForClose(pTrade.InstrumentID,
                            PosiDirection,
                            pTrade.HedgeFlag,
                            PositionDate,
                            pTrade.Volume) == 0;
                    }
                    else if (TThostFtdcOffsetFlagType.Close == pTrade.OffsetFlag)
                    {
                        return ReplaceForClose(pTrade.InstrumentID,
                            PosiDirection,
                            pTrade.HedgeFlag,
                            pTrade.Volume) == 0;
                    }
                    else
                    {
                        //无法计算的开平类型，要求直接发送查询请求
                        return false;
                    }
                }
            }
        }

        public void Clear()
        {
            lock(this)
            {
                dtInvestorPosition.Clear();
            }
        }

        public void Save()
        {
            //dtInvestorPosition.WriteXml("D:\\1.xml");
        }
    }
}

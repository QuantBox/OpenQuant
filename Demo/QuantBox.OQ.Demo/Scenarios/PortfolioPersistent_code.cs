using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using System.Xml.Serialization;
using System.IO;

namespace QuantBox.OQ.Demo.Scenarios
{
    public class PortfolioPersistent_code : Strategy
    {
        /// <summary>
        /// 持仓组合持久化
        /// 
        /// 按 策略_合约.xml 进行保存，用户可以自己编辑此文件
        /// </summary>
        public override void OnStrategyStart()
        {
            LoadPosition();
        }

        public override void OnStrategyStop()
        {
            SavePosition();
        }

        private void LoadPosition()
        {
            MyPosition pos = new MyPosition();
            XmlSerializer serializer = new XmlSerializer(pos.GetType());

            string path = string.Format("{0}_{1}.xml", Name, Instrument);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    pos = (MyPosition)serializer.Deserialize(stream);
                    stream.Close();

                    if (pos.Amount != 0)
                    {
                        Portfolio.Add(pos.EntryDate, pos.Amount > 0 ? TransactionSide.Buy : TransactionSide.Sell,
                            Math.Abs(pos.Amount), Instrument, pos.Price, "从XML中初始化");
                    }

                    Console.WriteLine(string.Format("加载路径:{0},持仓:{1},价格:{2}", path,pos.Amount,pos.Price));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SavePosition()
        {
            MyPosition pos = new MyPosition();
            XmlSerializer serializer = new XmlSerializer(pos.GetType());

            string path = string.Format("{0}_{1}.xml", Name, Instrument);
            using (TextWriter writer = new StreamWriter(path))
            {
                if (null == Position)
                {
                    pos.EntryDate = Clock.Now;
                    pos.Amount = 0;
                    pos.Price = 0;
                }
                else
                {
                    pos.EntryDate = Position.EntryDate;
                    pos.Amount = Position.Amount;
                    pos.Price = Position.GetPrice();
                }

                serializer.Serialize(writer, pos);
                writer.Close();

                Console.WriteLine(string.Format("保存路径:{0},持仓:{1},价格:{2}", path, pos.Amount, pos.Price));
            }
        }
    }
}

public class MyPosition
{
    public DateTime EntryDate;
    public double Amount;
    public double Price;
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using QuantBox.OQ.Demo.Module;

namespace QuantBox.OQ.Demo.Msic
{
    /// <summary>
    /// 在TB或MC一类的软件中使用FileAppend记下目标仓位，由OQ发单
    /// </summary>
    public class TB_code : TargetPositionModule
    {
        FileSystemWatcher watcher;

        private void WatcherStrat(string path, string filter)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Filter = filter;

            watcher.Changed += new FileSystemEventHandler(OnProcess);

            watcher.EnableRaisingEvents = true;
        }

        private void WatcherStop()
        {
            watcher.Changed -= new FileSystemEventHandler(OnProcess);

            watcher.EnableRaisingEvents = false;
        }

        private void OnProcess(object source, FileSystemEventArgs e)
        {
            // 比较无语，保存一次文本会触发两次
            // 猜测是因为保存一次，写修改时间一次
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Console.WriteLine("{0},{1}", e.ChangeType, e.FullPath);

                ParseLine(GetLastLine(e.FullPath));

                // 调用父类的下单处理
                Process();
            }
        }

        public override void OnStrategyStart()
        {
            base.OnStrategyStart();

            WatcherStrat(@"E:\", string.Format("{0}.log",Instrument.Symbol));
        }

        public override void OnStrategyStop()
        {
            base.OnStrategyStop();

            WatcherStop();
        }

        public string GetLastLine(string filepath)
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                string st = string.Empty;
                while (!sr.EndOfStream)
                {
                    st = sr.ReadLine();
                }

                Console.WriteLine("最后一行内容为：");
                Console.WriteLine(st);
                return st;
            }
        }

        public void ParseLine(string line)
        {
            try
            {
                // 这个地方最最后一行只有一个目标仓位
                base.TargetPosition = double.Parse(line);
            }
            catch (Exception)
            {
                // 没有识别出来就啥都不做
            }
        }
    }
}



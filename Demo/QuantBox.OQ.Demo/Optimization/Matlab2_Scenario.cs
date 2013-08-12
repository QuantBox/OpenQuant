using System;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Engine;
using System.Threading;
using System.IO;

namespace QuantBox.OQ.Demo.Optimization
{
    /// <summary>
    /// 使用内置的Simple Moving Average Crossover进行演示
    /// 
    /// 这个示例的主要目的是为了演示策略与Matlab的互动
    /// 
    /// 参考http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=111
    /// 要参加Interop.MLApp引用
    /// </summary>
    public class Matlab2_Scenario : Scenario
    {
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private void WatcherStrat(string path, string filter)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Filter = filter;

            watcher.Created += new FileSystemEventHandler(OnProcess);

            watcher.EnableRaisingEvents = true;
        }

        private void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Console.WriteLine("{0},{1}", e.ChangeType, e.FullPath);
                // 运行回测
                Start();
                // 解除等待
                _autoResetEvent.Set();
            }
        }


        public override void Run()
        {
            MLApp.MLAppClass matlab = new MLApp.MLAppClass();
            matlab.Visible = 1; //会显示Command Window

            matlab.Execute("clc; clear all; close all;");
            WatcherStrat(@"D:\test", "*.*");

            Project project = Solution.Projects[0];

            int line = 0;

            for (int length1 = 3; length1 <= 7; length1++)
            {
                for (int length2 = 3; length2 <= 7; length2++)
                {
                    if (length2 > length1)
                    {
                        // set new parameters
                        project.Parameters["Length1"].Value = length1;
                        project.Parameters["Length2"].Value = length2;

                        // print parameters
                        Console.Write("Length1 = " + length1 + " Length2 = " + length2);

                        // start backtest 
                        // 这句话要写到matlab中的函数中去
                        matlab.Execute(@"save D:\test\" + Clock.Now.Ticks);
                        // 等待策略跑完一次
                        _autoResetEvent.WaitOne();

                        // calculate objective function 
                        double objective = Solution.Portfolio.GetValue();
                        // print objective 

                        Console.WriteLine(" Objective = " + objective);
                        // check best objective
                    }
                }
            }

            // 执行完后暂时不退出
            //matlab.Quit();
            //matlab = null;
        }
    }
}

/*

% 从OpenQuant优化过后的结果导出成Excel,然后画参数与绩效的二维图，以下是两个参数的示例
clc; clear all; close all;
A=xlsread('模拟退火算法.xlsx','Optimization_History');
x=A(:,3);y=A(:,4);z=A(:,2);
t1=linspace(min(x),max(x),max(x)-min(x)); % 如果数据不多，最后的max(x)-min(x)+1可以改成50等
t2=linspace(min(y),max(y),max(y)-min(y)); % 同上
[X,Y]=meshgrid(t1,t2);
Z=griddata(x,y,z,X,Y,'v4');
figure,surfc(X,Y,Z),colorbar
xlabel('x');grid on;
ylabel('y');grid on;
zlabel('z');grid on;
title('Performance');


 */
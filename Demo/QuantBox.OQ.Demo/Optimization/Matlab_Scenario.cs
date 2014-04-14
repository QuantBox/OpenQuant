using System;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Engine;

namespace QuantBox.OQ.Demo.Optimization
{
    /// <summary>
    /// 使用内置的Simple Moving Average Crossover进行演示
    /// 
    /// 使用Matlab进行3D绩效绘图
    /// 
    /// 参考http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=111
    /// 要参加Interop.MLApp引用
    /// </summary>
    public class Matlab_Scenario : Scenario
    {/*
        public override void Run()
        {
            MLApp.MLAppClass matlab = new MLApp.MLAppClass();
            matlab.Visible = 1; //会显示Command Window

            matlab.Execute("clc; clear all; close all;");

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
                        Start();

                        // calculate objective function 
                        double objective = Solution.Portfolio.GetValue();
                        // print objective 

                        Console.WriteLine(" Objective = " + objective);
                        // check best objective

                        matlab.Execute(string.Format("x({0})={1}; y({0})={2}; z({0})={3};", ++line, length1, length2, objective));
                    }
                }
            }

            matlab.Execute("t1=linspace(min(x),max(x),max(x)-min(x)+1); %% 如果数据不多，最后的max(x)-min(x)+1可以改成50等");
            matlab.Execute("t2=linspace(min(y),max(y),max(y)-min(y)+1); %% 同上");
            matlab.Execute("[X,Y]=meshgrid(t1,t2);");
            matlab.Execute("Z=griddata(x,y,z,X,Y,'v4');");
            matlab.Execute("figure,surfc(X,Y,Z),colorbar");
            matlab.Execute("xlabel('x'); ylabel('y'); zlabel('z');");
            matlab.Execute("title('Performance');");

            matlab.Execute("save"); //保存一下，用于后期分析

            // 执行完后暂时不退出
            //matlab.Quit();
            //matlab = null;
        }
    */}
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
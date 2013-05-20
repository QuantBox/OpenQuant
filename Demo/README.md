# Demo项目

## 目的
1. 提供示例
2. 共同维护基本代码

## 使用方法
1. 使用VS2012打开解决方案，指定Build->Output Path为OpenQuant下的Bin目录
2. 设置好此解决方案引用的dll的路径
3. 在OpenQuant，Options->Build中引入QuantBox.OQ.Demo.dll
4. 使用代码如下
> using System;<br/>
using System.Drawing;<br/>
using OpenQuant.API;<br/>
using OpenQuant.API.Indicators;<br/>
<br/>
using QuantBox.OQ.Demo.Indicator;<br/>
using QuantBox.OQ.Demo.Indicator.Test;<br/>
<br/>
public class MyStrategy : KDJ_code
{
}
# OpenQuant内盘期货插件

## 目的
将OpenQuant与国内的CTP进行对接，让OpenQuant直接能交易国内期货

## 设计思路
1. 利用了本开源项目的C-CTP接口，与CSharp-C接口
2. C-CTP以dll方式调用
3. 本插件同时支持QuantDeveloper、OpenQuant2和OpenQuant3（以下分别简称QD、OQ2和OQ3），只要进行再编译即可
4. 为了简化编译过程直接将CSharp-C接口源码复制到OQ插件源码中
5. 为了支持查询合约列表功能，不使用OpenQuant接口，而是使用更底层的SmartQuant接口

## 如何安装使用
1. 找到软件的插件目录C:\Program Files\SmartQuant Ltd\OpenQuant\Framework\bin\
2. 复制thostmduserapi.dll、thosttraderapi.dll两个CTP的dll到指定路径
3. 复制QuantBox.C2CTP.dll这个C-CTP接口的dll
4. 复制QuantBox.OQ.CTP.dll这个OQ插件，确保此插件的版本正确
5. 找到软件的插件配置文件C:\Documents and Settings\Administrator\Application Data\SmartQuant Ltd\OpenQuant\Framework\ini\framework.xml
6. 添加< plugin enabled="True" assembly="QuantBox.OQ.CTP" type="QuantBox.OQ.CTP.QBProvider" x64="False" />到对应位置
7. 如何使用请查看插件的使用说明

## 如何开发
1. 确保你的C-CTP接口的dll是最新的
2. 确保工程下CSharp2C的源码也是从CSharp-C项目下源码中复制出来的最新的
3. 修改引用中有关SmartQuant类库的地址，使用你目标OQ中下的dll
4. 修改.NET框架要使用的版本，QD使用2.0，OQ2使用3.5，OQ3使用4.0
5. 修改dll生成的目录，具体请参考如何安装。
6. 调试只能使用附加到进程，建议学习并使用远程调试
7. 如果插件完全无法加载，请找到对应的log文件，查看日志。

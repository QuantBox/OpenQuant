<html>
<head>
<title>SmartQuant内盘期货插件</title>
</head>
<body>
<h1>授权</h1>
待定

<h1>安装</h1>
<ol>
<li>下载并安装OpenQuant 3.x 32bit(由于上期技术只提供了Win32的dll,所以OpenQuant也只能用32位)</li>
<li>下载并安装SmartQuant内盘期货插件，执行此步前保证OpenQuant已经运行过一次</li>
<li>选择自己期货公司，会自动下载期货公司的服务器配置信息</li>
<li>如果找到不自己所在的经纪公司，有可能还未与本团队合作，您可以督促下您的客户经理。您也可以手工配置。</li>
</ol>

<h1>配置</h1>
<ol>
<li>开启OpenQuant，进入到Tools->Options->Modes，将Paper和Live中的Market Data改成CTP，将Live中的Execution Provider也改成CTP</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/1.png"/>
<li>进入到Views->Providers,找到CTP，右键选择“Properties”</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/2.png"/>
<li>在Properties面板中的“Account”和“Server”中添加自己的账号和服务器</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/3.png"/>
<li>选中“Account”,添加账号信息，可以添加多个，目前只对第一条记录进行登录</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/4.png"/>
<li>选中“Server”,添加服务器信息，可以添加多个，目前只对第一条记录进行登录</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/5.png"/>
<li>在“MarketData”中添加行情服务器地址，在"Trading"中添加交易服务器地址，支持多服务器，格式为tcp://xxx.xxx...，或udp://xxx.xxx...</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/6.png"/>
<li>"DataTimeMode"表示时间模式，交易时请使用"LocalTime",行情收集时请使用"ExchangeTime"。因为交易时使用ExchangeTime会导致Bar生成错误</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/3.png"/>
</ol>


<h1>使用</h1>
<ol>
<li>在“Properties”面板中直接选择“CTP”后“Connect”即可，在“Output”中可以查看连接日志</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/7.png"/>
<li>在Data->Import->Instruments->CTP，可以打开合约导入对话框。支持关键字过滤。</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/8.png"/>
<li>导入合约的方式自动填写了“PriceFormat”和“TickSize”两个属性，这两个属性用于插件报单时对报单价进行规范化处理，不能有错，否则报单出错</li>
<img src="https://github.com/QuantBox/QuantBox/raw/master/OpenQuant/Provider/www/9.png"/>
<li>查看行情和交易的方式请查看OpenQuant的帮助文档</li>
</ol>
</body>
</html>

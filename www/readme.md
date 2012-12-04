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
<li>到快期软件中查找地址信息</li>
</ol>

<h1>配置</h1>
<ol>
<li>开启OpenQuant，进入到Tools->Options->Modes，将Paper和Live中的Market Data改成CTP，将Live中的Execution Provider也改成CTP</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/1.png"/>
<li>进入到Views->Providers,找到CTP，右键选择“Properties”</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/2.png"/>
<li>在Properties面板中的“Account”和“Server”中添加自己的账号和服务器</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/3.png"/>
<li>选中“Account”,添加账号信息，可以添加多个，目前只对第一条记录进行登录</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/4.png"/>
<li>选中“Server”,添加服务器信息，可以添加多个，目前只对第一条记录进行登录</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/5.png"/>
<li>在“MarketData”中添加行情服务器地址，在"Trading"中添加交易服务器地址，支持多服务器，格式为tcp://xxx.xxx...，或udp://xxx.xxx...</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/6.png"/>
<li>"DataTimeMode"表示时间模式，交易时请使用"LocalTime",行情收集时请使用"ExchangeTime"。因为交易时使用ExchangeTime会导致Bar生成错误</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/3.png"/>
</ol>


<h1>使用</h1>
<ol>
<li>在“Providers”面板中直接选择“CTP”后“Connect”即可，在“Output”中可以查看连接日志</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/7.png"/>
<li>在Data->Import->Instruments->CTP，可以打开合约导入对话框。支持关键字过滤。</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/8.png"/>
<li>导入合约的方式自动填写了“PriceFormat”和“TickSize”等属性</li>
<img src="https://raw.github.com/QuantBox/OpenQuant/master/www/9.png"/>
<li>查看行情和交易的方式请查看OpenQuant的帮助文档</li>
</ol>
</body>
</html>

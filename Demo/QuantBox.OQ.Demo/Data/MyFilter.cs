﻿using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Engine;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;
using System.Collections.Generic;
using QuantBox.OQ.Demo.Helper;

namespace QuantBox.OQ.Demo.Data
{
    public class MyFilter : MarketDataFilter
    {
        Dictionary<string, Trade> goodTrades = new Dictionary<string, Trade>();
        Dictionary<string, TimeHelper> TimeHelpers = new Dictionary<string, TimeHelper>();

        bool isRightTime(string symbol, DateTime datetime)
        {
            int nDatetime = datetime.Hour * 100 + datetime.Minute;

            TimeHelper th;
            if(!TimeHelpers.TryGetValue(symbol,out th))
            {
                th = new TimeHelper(symbol);
                TimeHelpers[symbol] = th;
            }

            return th.IsTradingTime(datetime);
        }

        public override Bar FilterBarOpen(Bar bar, string symbol)
        {
            // 只过滤Time Bar
            if (bar.Type != BarType.Time)
                return bar;

            // 日线数据不过滤
            if (bar.Size >= 86400)
                return bar;

            if (isRightTime(symbol, bar.DateTime))
                return bar;

            return null;
        }

        public override Bar FilterBar(Bar bar, string symbol)
        {
            if (bar.Type != BarType.Time)
                return bar;

            if (bar.Size >= 86400)
                return bar;

            // 注意！！！！！
            // 商品是13点30中中午开始，而如果你设置的是1小时一个Bar
            // 只使用DateTime进行判断会导致1点到2点的Bar被丢弃
            // 所以要多判断些条件
            if (isRightTime(symbol, bar.DateTime)
                || isRightTime(symbol, bar.EndTime)
                || isRightTime(symbol, bar.BeginTime))
                return bar;

            return null;
        }

        public override Trade FilterTrade(Trade trade, string symbol)
        {
            // 进行Trade过滤下面只给一个示例
            return trade;

            Trade lastGoodTrade = null;

            //检查数据表中是否有正常交易
            if (goodTrades.TryGetValue(symbol, out lastGoodTrade))
            {
                // 检查是否有数据特征

                //检查新的交易与上一个正常交易相比，价差是否在0.5%以上
                if (Math.Abs((1 - lastGoodTrade.Price / trade.Price) * 100) < 0.5)
                {
                    //是正常交易，就更新为上一个正常交易
                    goodTrades[symbol] = trade;
                    return trade;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // 将第一次交易数据作为初始正常数据
                goodTrades[symbol] = trade;
                // 将此数据添加到数据表里（第一个数据）
                return trade;
            }
        }

        public override Quote FilterQuote(OpenQuant.API.Quote quote, string symbol)
        {
            // 接收所有报价
            return quote;
        }
    }
}

/*
注意:只能对一个Provider设置一个Filter,重复设置会覆盖

1.在Scenario中使用
MarketDataProvider.Filter = new MyFilter();

2.在Script中使用
MarketDataProvider provider = (MarketDataProvider)ProviderManager.Providers["CTP"];
provider.Filter = new MyFilter();

3.在Strategy中使用
MarketDataProvider.Filter = new MyFilter();

 */

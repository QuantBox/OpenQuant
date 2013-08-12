// QunatBox.OQ.Demo.CLI.h

#pragma once

using namespace System;

namespace QuantBoxOQDemoCLI {

	public class UnmanagedClass
	{
	public:
		virtual void OnBarOpen(OpenQuant::API::Bar^ bar);
		virtual void OnBar(OpenQuant::API::Bar^ bar);
	protected:
	private:
	};

	public ref class ManagedClass: public OpenQuant::API::Strategy
	{
	public:
		virtual void OnStrategyStart() override;
		virtual void OnBarOpen(OpenQuant::API::Bar^ bar) override;
		virtual void OnBar(OpenQuant::API::Bar^ bar) override;

	public:
		ManagedClass():m_Impl( new UnmanagedClass ){}
		~ManagedClass(){delete m_Impl;}
	protected:
		!ManagedClass(){delete m_Impl;}
	private:
		UnmanagedClass * m_Impl;
	};
}

/*
添加引用OpenQuant.API.dll

编译成dll,复制到OpenQuant\Bin目录下

在OpenQuant的策略文件中code.cs中这样写
using OpenQuant.API;
using QunatBoxOQDemoCLI;

public class MyStrategy : ManagedClass
{
}

用Reflector也看不到UnmanageClass这个类中的具体代码了。
*/

// This is the main DLL file.

#include "stdafx.h"

#include "QuantBox.OQ.Demo.CLI.h"

using namespace QuantBoxOQDemoCLI;

void ManagedClass::OnStrategyStart()
{
	Console::WriteLine(Instrument->Symbol);
}

void ManagedClass::OnBarOpen(OpenQuant::API::Bar^ bar)
{
	m_Impl->OnBarOpen(bar);
}

void ManagedClass::OnBar(OpenQuant::API::Bar^ bar)
{
	m_Impl->OnBar(bar);
}

void UnmanagedClass::OnBarOpen(OpenQuant::API::Bar^ bar)
{
	Console::WriteLine(bar);
}

void UnmanagedClass::OnBar(OpenQuant::API::Bar^ bar)
{
	Console::WriteLine(bar);
}
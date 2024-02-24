#pragma once
#include "Server.h"
#include "Protocol/Protocol.hpp"
#include "Global/LogText.hpp"

int main()
{
	SetConsoleOutputCP( CP_UTF8 );

	LogText::Inst().alignment  = LogAlignment::All;
	//LogText::Inst().ignoreData = true;

	Server server( 10000 );
	return 0;
}
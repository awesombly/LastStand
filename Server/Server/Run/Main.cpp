#pragma once
#include "Server.h"
#include "Protocol/Protocol.hpp"
#include "Global/LogText.hpp"

int main()
{
	SetConsoleOutputCP( CP_UTF8 );

	Debug.alignment  = LogAlignment::All;
	Debug.writeType  = LogWriteType::All;
	Debug.ignoreData = false;

	Server server;
	server.Start( 10000 );

	return 1;
}
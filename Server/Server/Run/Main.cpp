#pragma once
#include "Server.h"
#include "Protocol/Protocol.hpp"

int main()
{
	SetConsoleOutputCP( CP_UTF8 );

	Server server;
	server.Start( 10000 );

	return 1;
}
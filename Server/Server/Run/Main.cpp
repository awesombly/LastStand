#pragma once
#include "Server.h"
#include "../Protocol/Protocol.h"

int main()
{
	//Protocol  p;

	SampleProtocol1 p1;
	SampleProtocol2 p2;

	Server server;
	server.Start( 10000 );

	return 1;
}
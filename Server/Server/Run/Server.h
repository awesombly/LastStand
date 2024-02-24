#pragma once
#include "Network/Acceptor.h"

class Server
{
private:
	Acceptor acceptor;

public:
	Server( const int _port, const char* _address = 0 );
	virtual ~Server() = default;
};
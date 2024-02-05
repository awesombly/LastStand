#pragma once
#include "Connect/Acceptor.h"

class Server
{
private:
	Acceptor acceptor;
	HANDLE kill;

public:
	Server();
	virtual ~Server() = default;

public:
	void Start( const int _port, const char* _address = 0 );
};

#pragma once
#include "../Network/Network.h"

class Session : public Network
{
private:
	PACKET* packet;
	byte temp[MaxReceiveSize];
	u_int writePos, readPos;
	u_int startPos;

public:
	Session() = default;
	Session( const SOCKET& _socket, const SOCKADDR_IN& _address );
	virtual ~Session();

public:
	void Dispatch( const LPOVERLAPPED& _ov, DWORD _size );
};
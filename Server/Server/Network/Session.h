#pragma once
#include "Network/Network.h"

static const u_short MaxReceiveSize = 10000;

class Session : public Network
{
private:
	int unresponse;
	std::chrono::system_clock::time_point lastResponseTime;

	static const int   MaxUnresponse;
	static const float MinResponseWaitTime;
	static const float RequestDelay;
	std::chrono::duration<float> time;

	UPacket* packet;
	byte buffer[MaxReceiveSize];
	u_int startPos, writePos, readPos;

public:
	bool isPlaying;
	LOGIN_INFO loginInfo;

public:
	Session() = default;
	Session( const SOCKET& _socket, const SOCKADDR_IN& _address );
	virtual ~Session() override;

private:
	void Alive();

public:
	bool CheckAlive();
	void Dispatch( const LPOVERLAPPED& _ov, DWORD _size );
};
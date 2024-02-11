#pragma once
#include "../../Connect/Session.h"

class Room
{
private:
	RoomInfo info;
	Session* host;
	std::list<Session*> sessions;

public:
	Room( const SOCKET& _host, const RoomInfo& _info );
	virtual ~Room() = default;

public:
	const RoomInfo& GetInfo() const;
};
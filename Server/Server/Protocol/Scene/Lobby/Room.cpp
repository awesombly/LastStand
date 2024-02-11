#include "Room.h"
#include "Managed/SessionManager.h"

Room::Room( u_short _uid, const SOCKET& _host, const RoomData& _data ) : 
	        uid( _uid ), data( _data )
{
	host = SessionManager::Inst().Find( _host );
	sessions.push_back( host );
}

const RoomData& Room::GetData() const { return data; }

const u_short& Room::GetUID() const { return uid; }
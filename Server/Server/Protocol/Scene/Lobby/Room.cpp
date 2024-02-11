#include "Room.h"
#include "Managed/SessionManager.h"

Room::Room( const SOCKET& _host, const RoomInfo& _info ) : info( _info )
{
	host = SessionManager::Inst().Find( _host );
	sessions.push_back( host );
}

const RoomInfo& Room::GetInfo() const { return info; }
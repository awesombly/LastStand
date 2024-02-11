#include "Lobby.h"
#include "Managed/ProtocolSystem.h"
#include "Managed/SessionManager.h"

u_short Lobby::RoomUID = 0;
std::list<Room*> Lobby::rooms;

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( ReqMakeRoom(), MakeRoom );
}

void Lobby::MakeRoom( const Packet& _packet )
{
	ReqMakeRoom data = FromJson<ReqMakeRoom>( _packet );
	ResMakeRoom ret;
	ret.isCompleted = data.title.size() > 0;
	if ( ret.isCompleted )
	{
		Room* room = new Room( RoomUID++, _packet.socket, RoomData( data.title, data.maxPersonnel ) );
		ret.uid = room->GetUID();
		rooms.push_back( room );

		std::cout << "Create a new room( " << rooms.size() << " )" << std::endl;
	}

	SessionManager::Inst().Send( _packet.socket, ret );
}
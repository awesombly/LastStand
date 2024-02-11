#include "Lobby.h"
#include "Managed/ProtocolSystem.h"
#include "Managed/SessionManager.h"

u_short Lobby::RoomUID = 0;
std::list<Room*> Lobby::rooms;
std::list<RoomData> Lobby::roomDatas;

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( CREATE_ROOM_REQ, MakeRoom );
}

void Lobby::MakeRoom( const Packet& _packet )
{
	ReqMakeRoom data = FromJson<ReqMakeRoom>( _packet );
	ResMakeRoom ret;
	ret.isCompleted = data.title.size() > 0;
	if ( ret.isCompleted )
	{
		RoomData roomData( data.title, data.maxPersonnel );
		Room* room = new Room( RoomUID++, _packet.socket, roomData );
		ret.uid = room->GetUID();
		rooms.push_back( room );
		roomDatas.push_back( roomData );


		ResTakeRoom takeRoom;
		takeRoom.rooms = roomDatas;

		SessionManager::Inst().Send( _packet.socket, UPacket( TAKE_ROOM_LIST, takeRoom ) );

		std::cout << "Create a new room( " << rooms.size() << " )" << std::endl;
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( CREATE_ROOM_RES, ret ) );
}
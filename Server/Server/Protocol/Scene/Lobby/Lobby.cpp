#include "Lobby.h"
#include "Managed/ProtocolSystem.h"
#include "Managed/SessionManager.h"

u_short Lobby::RoomUID = 0;
std::list<Room*> Lobby::rooms;
std::list<RoomInfo> Lobby::roomInfos;

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( CREATE_ROOM_REQ, CreateRoom );
}

void Lobby::CreateRoom( const Packet& _packet )
{
	ROOM_INFO data = FromJson<ROOM_INFO>( _packet );
	CONFIRM confirm;
	confirm.isCompleted = data.title.size() > 0;
	if ( confirm.isCompleted )
	{
		RoomInfo roomData;
		roomData.uid       = RoomUID++;
		roomData.title     = data.title;
		roomData.personnel = data.personnel;

		rooms.push_back( new Room( _packet.socket, roomData ) );
		roomInfos.push_back( roomData );

		//ResTakeRoom takeRoom;
		//takeRoom.rooms = roomInfos;
		//SessionManager::Inst().Send( _packet.socket, UPacket( TAKE_ROOM_LIST, takeRoom ) );
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( CREATE_ROOM_RES, confirm ) );
}
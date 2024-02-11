#include "Lobby.h"
#include "Managed/ProtocolSystem.h"
#include "Managed/SessionManager.h"

u_short Lobby::RoomUID = 0;
std::list<Room*> Lobby::rooms;
std::list<RoomInfo> Lobby::infos;

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( CREATE_ROOM_REQ, CreateRoom );
	ProtocolSystem::Inst().Regist( LOBBY_INFO_REQ,  TakeLobbyInfo );
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
		roomData.personnel.maximum = data.personnel.maximum;
		roomData.personnel.current = 1;

		rooms.push_back( new Room( _packet.socket, roomData ) );
		infos.push_back( roomData );
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( CREATE_ROOM_ACK, confirm ) );
}

void Lobby::TakeLobbyInfo( const Packet& _packet )
{
	LOBBY_INFO info;
	info.infos = infos;

	SessionManager::Inst().Send( _packet.socket, UPacket( LOBBY_INFO_ACK, info ) );
}
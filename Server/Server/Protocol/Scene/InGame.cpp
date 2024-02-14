#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,		 AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,		 AckExitStage );
	ProtocolSystem::Inst().Regist( SPAWN_PLAYER_REQ,	 AckSpawnPlayer );
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ,		 AckSpawnActor );
	ProtocolSystem::Inst().Regist( SYNK_MOVEMENT_REQ,	 AckSynkMovement );
	ProtocolSystem::Inst().Regist( INGAME_LOAD_DATA_REQ, AckInGameLoadData );
}

void InGame::AckChatMessage( const Packet& _packet )
{
	_packet.session->stage->Broadcast( _packet );
}

void InGame::AckExitStage( const Packet& _packet )
{
	Session* session = _packet.session;
	SessionManager::Inst().ExitStage( session );
	session->Send( UPacket( EXIT_STAGE_ACK, EMPTY() ) );
}

void InGame::AckSpawnPlayer( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( data.serial == 0 )
	{
		data.serial = Global::GetNewSerial();
	}

	if ( data.isLocal == false )
	{
		_packet.session->stage->Send( data.socket, UPacket( SPAWN_PLAYER_ACK, data ) );
		return;
	}

	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, data ) );
}

void InGame::AckSpawnActor( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( data.serial == 0 )
	{
		data.serial = Global::GetNewSerial();
	}
	SessionManager::Inst().Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

void InGame::AckSynkMovement( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	SessionManager::Inst().BroadcastWithoutSelf( _packet.session, UPacket( SYNK_MOVEMENT_ACK, data ) );
}

void InGame::AckInGameLoadData( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		std::cout << "stage is null. nick : " << _packet.session->loginInfo.nickname << std::endl;
		return;
	}

	if ( _packet.session->stage->host == nullptr )
	{
		std::cout << "host is null. nick : " << _packet.session->loginInfo.nickname << std::endl;
		return;
	}

	if ( _packet.session == _packet.session->stage->host )
	{
		return;
	}

	ACTOR_INFO data;
	::memset( &data, 0, sizeof( ACTOR_INFO ) );
	data.socket = _packet.session->GetSocket();
	_packet.session->stage->host->Send( UPacket( INGAME_LOAD_DATA_ACK, data ) );
}

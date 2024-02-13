#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( SPAWN_PLAYER_REQ, SpawnPlayer );
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ, SpawnActor );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,  AckExitStage );
	ProtocolSystem::Inst().Regist( SYNK_MOVEMENT_REQ, SynkMovement );
}

void InGame::SpawnPlayer( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	data.serial = Global::GetNewSerial();
	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.isLocal = false;
	SessionManager::Inst().BroadcastWithoutSelf( _packet.socket, UPacket( SPAWN_PLAYER_ACK, data ) );
}

void InGame::SpawnActor( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	data.serial = Global::GetNewSerial();
	SessionManager::Inst().Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

void InGame::AckExitStage( const Packet& _packet )
{
	Session* session = _packet.session;
	SessionManager::Inst().ExitStage( session );
	_packet.session->Send( UPacket( EXIT_STAGE_ACK, EMPTY() ) );
}

void InGame::SynkMovement( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	SessionManager::Inst().BroadcastWithoutSelf( _packet.socket, UPacket( SYNK_MOVEMENT_ACK, data ) );
}

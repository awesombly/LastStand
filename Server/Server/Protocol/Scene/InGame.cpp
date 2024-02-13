#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( SPAWN_PLAYER_REQ, SpawnPlayer );
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ, SpawnActor );
}

void InGame::SpawnPlayer( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	data.serial = Global::GetNewSerial();
	data.isLocal = true;
	SessionManager::Inst().Broadcast( UPacket( SPAWN_PLAYER_ACK, data ) );
	/// 발신자만 Send 기능 필요
	///data.isLocal = false;
	///SessionManager::Inst().BroadcastWithoutSelf( _packet.socket, UPacket( SPAWN_PLAYER_ACK, data ) );
}

void InGame::SpawnActor( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	data.serial = Global::GetNewSerial();
	SessionManager::Inst().Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

#include "InGame.h"
#include "Managed/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( SPAWN_ENEMY_REQ, SpawnEnemy );
}

void InGame::SpawnEnemy( const Packet& _packet )
{
	SPAWN_ENEMY data = FromJson<SPAWN_ENEMY>( _packet );
	SessionManager::Inst().Broadcast( UPacket( SPAWN_ENEMY_ACK, data ) );
}
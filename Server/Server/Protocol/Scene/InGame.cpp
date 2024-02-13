#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,  AckExitStage );
	ProtocolSystem::Inst().Regist( SPAWN_ENEMY_REQ, SpawnEnemy );
}

void InGame::AckExitStage( const Packet& _packet )
{

}

void InGame::SpawnEnemy( const Packet& _packet )
{
	SPAWN_ENEMY data = FromJson<SPAWN_ENEMY>( _packet );
	SessionManager::Inst().Broadcast( UPacket( SPAWN_ENEMY_ACK, data ) );
}
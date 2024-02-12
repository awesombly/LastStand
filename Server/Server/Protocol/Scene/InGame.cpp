#include "InGame.h"
#include "Managed/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ, SpawnEnemy );
}

void InGame::SpawnEnemy( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	data.serial = Global::GetNewSerial();
	SessionManager::Inst().Broadcast( _packet.socket, UPacket( SPAWN_ACTOR_ACK, data ) );
}

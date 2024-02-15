#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,		 AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,		 AckExitStage );
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

void InGame::AckSpawnActor( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		std::cout << __FUNCTION__ << " : Stage is null. nick:" << _packet.session->loginInfo.nickname << std::endl;
		return;
	}

	data.socket = _packet.session->GetSocket();
	if ( data.serial == 0 )
	{
		data.serial = Global::GetNewSerial();
	}

	ActorInfo* actor = new ActorInfo( data );
	_packet.session->stage->RegistActor( actor );

	_packet.session->stage->Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

void InGame::AckSynkMovement( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );

	ActorInfo* actor = _packet.session->stage->GetActor( data.serial );
	if ( actor == nullptr )
	{
		std::cout << __FUNCTION__ << " : Actor is null. serial:" << data.serial << std::endl;
		return;
	}

	actor->position = data.position;
	actor->rotation = data.rotation;
	actor->velocity = data.velocity;

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNK_MOVEMENT_ACK, data ) );
}

void InGame::AckInGameLoadData( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		std::cout << "Stage is null. nick : " << _packet.session->loginInfo.nickname << std::endl;
		return;
	}

	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( data.serial == 0 )
	{
		data.serial = Global::GetNewSerial();
	}

	// 접속시 플레이어 생성
	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, data ) );

	// 기존에 있던 Player 스폰
	ActorContainer actors = _packet.session->stage->GetActors();
	for ( auto pair : actors )
	{
		if ( pair.second == nullptr )
		{
			std::cout << __FUNCTION__ << " : Actor is null." << std::endl;
			return;
		}

		_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, *pair.second ) );
	}

	// 기존 데이터 스폰후 등록
	ActorInfo* player = new ActorInfo( data );
	_packet.session->stage->RegistActor( player );
}

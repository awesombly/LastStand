#include "InGame.h"
#include "Management/SessionManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,		 AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,		 AckExitStage );
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ,		 AckSpawnActor );
	ProtocolSystem::Inst().Regist( SPAWN_BULLET_REQ,	 AckSpawnBullet );
	ProtocolSystem::Inst().Regist( REMOVE_ACTOR_REQ,	 AckRemoveActor );
	ProtocolSystem::Inst().Regist( SYNK_MOVEMENT_REQ,	 AckSynkMovement );
	ProtocolSystem::Inst().Regist( SYNK_RELOAD_REQ,		 AckSynkReload );
	ProtocolSystem::Inst().Regist( SYNK_LOOK_REQ,		 AckSynkLook );
	ProtocolSystem::Inst().Regist( HIT_ACTOR_REQ,		 AckHitActor );
	ProtocolSystem::Inst().Regist( INGAME_LOAD_DATA_REQ, AckInGameLoadData );
}

void InGame::AckChatMessage( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		return;
	}
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
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	if ( data.serial == 0 )
	{
		data.serial = Global::GetNewSerial();
	}

	ActorInfo* actor = new ActorInfo( data );
	_packet.session->stage->RegistActor( actor );

	_packet.session->stage->Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

void InGame::AckSpawnBullet( const Packet& _packet )
{
	BULLET_INFO data = FromJson<BULLET_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. owner:", data.owner, ",nick:", _packet.session->loginInfo.nickname );
		return;
	}

	data.serial = Global::GetNewSerial();
	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_BULLET_ACK, data ) );
	data.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_BULLET_ACK, data ) );

	ActorInfo* actor = new ActorInfo();
	actor->prefab = data.prefab;
	actor->isLocal = data.isLocal;
	actor->serial = data.serial;
	_packet.session->stage->RegistActor( actor );
}

void InGame::AckRemoveActor( const Packet& _packet )
{
	SERIAL_INFO data = FromJson<SERIAL_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( REMOVE_ACTOR_ACK, data ) );

	ActorInfo* actor = _packet.session->stage->GetActor( data.serial );
	_packet.session->stage->UnregistActor( actor );
	Global::Memory::SafeDelete( actor );
}

void InGame::AckSynkMovement( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	ActorInfo* actor = _packet.session->stage->GetActor( data.serial );
	if ( actor == nullptr )
	{
		Debug.LogError( "Actor is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	actor->position = data.position;
	actor->rotation = data.rotation;
	actor->velocity = data.velocity;

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNK_MOVEMENT_ACK, data ) );
}

void InGame::AckSynkReload( const Packet& _packet )
{
	SERIAL_INFO data = FromJson<SERIAL_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNK_RELOAD_ACK, data ) );
}

void InGame::AckSynkLook( const Packet& _packet )
{
	LookInfo data = FromJson<LOOK_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNK_LOOK_ACK, data ) );
}

void InGame::AckHitActor( const Packet& _packet )
{
	HIT_INFO data = FromJson<HIT_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Session is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( HIT_ACTOR_ACK, data ) );

	if ( data.needRelease )
	{
		ActorInfo* bullet = _packet.session->stage->GetActor( data.bullet );
		if ( bullet == nullptr )
		{
			Debug.LogError( "Bullet is null. serial:", data.bullet, ", nick:", _packet.session->loginInfo.nickname );
			return;
		}

		_packet.session->stage->UnregistActor( bullet );
		Global::Memory::SafeDelete( bullet );
	}
}

void InGame::AckInGameLoadData( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	PLAYER_INFO data = FromJson<PLAYER_INFO>( _packet );
	if ( data.actorInfo.serial == 0 )
	{
		data.actorInfo.serial = Global::GetNewSerial();
	}

	// 접속시 플레이어 생성
	data.nickname = _packet.session->loginInfo.nickname;
	data.actorInfo.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.actorInfo.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, data ) );

	// 기존에 있던 Player 스폰
	std::list<Session*> sessions = _packet.session->stage->GetSessions();
	for ( auto session : sessions )
	{
		if ( session == nullptr )
		{
			Debug.LogError( "Session is null. " );
			return;
		}

		if ( session == _packet.session
			|| session->player == nullptr )
		{
			continue;
		}

		_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, *session->player ) );
	}

	// 기존 데이터 스폰후 등록
	PlayerInfo* player = new PlayerInfo( data );
	_packet.session->player = player;
	_packet.session->stage->RegistActor( &player->actorInfo );
}

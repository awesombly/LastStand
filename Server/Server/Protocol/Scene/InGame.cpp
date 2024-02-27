#include "InGame.h"
#include "Management/SessionManager.h"
#include "Management/StageManager.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,			AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,			AckExitStage );
	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ,			AckSpawnActor );
	ProtocolSystem::Inst().Regist( SPAWN_PLAYER_REQ,		AckSpawnPlayer );
	ProtocolSystem::Inst().Regist( SPAWN_BULLET_REQ,		AckSpawnBullet );
	ProtocolSystem::Inst().Regist( REMOVE_ACTORS_REQ,		AckRemoveActors );
	ProtocolSystem::Inst().Regist( SYNC_MOVEMENT_REQ,		AckSyncMovement );
	ProtocolSystem::Inst().Regist( SYNC_RELOAD_REQ,			AckSyncReload );
	ProtocolSystem::Inst().Regist( SYNC_LOOK_ANGLE_REQ,		AckSyncLook );
	ProtocolSystem::Inst().Regist( SYNC_DODGE_ACTION_REQ,	AckSyncDodgeAction );
	ProtocolSystem::Inst().Regist( SYNC_SWAP_WEAPON_REQ,	AckSyncSwapWeapon );
	ProtocolSystem::Inst().Regist( HIT_ACTORS_REQ,			AckHitActors );
	ProtocolSystem::Inst().Regist( INGAME_LOAD_DATA_REQ,	AckInGameLoadData );
}

void InGame::AckChatMessage( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
		 return;

	_packet.session->stage->Broadcast( _packet );
}

void InGame::AckExitStage( const Packet& _packet )
{
	Session* session = _packet.session;
	STAGE_INFO data  = FromJson<STAGE_INFO>( _packet );

	try
	{
		Stage* stage = StageManager::Inst().Find( data.serial );
		if ( stage == nullptr )
			 throw std::exception( "# This stage does not exist" );

		if ( !stage->Exit( session ) )
		{
			// 방에 아무도 없을 때
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( DELETE_STAGE_INFO, stage->info ) );
			StageManager::Inst().Erase( stage );
		}
		else
		{
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}

		session->Send( UPacket( EXIT_STAGE_ACK, EMPTY() ) );
	}
	catch ( std::exception _error )
	{
		Debug.LogWarning( _error.what() );
	}
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

void InGame::AckSpawnPlayer( const Packet& _packet )
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

	data.nickname = _packet.session->loginInfo.nickname;
	data.actorInfo.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.actorInfo.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, data ) );

	if ( _packet.session->player == nullptr )
	{
		PlayerInfo* player = new PlayerInfo( data );
		_packet.session->player = player;
		_packet.session->stage->RegistActor( &player->actorInfo );
	}
	else
	{
		_packet.session->player->actorInfo = data.actorInfo;
		_packet.session->player->isDead = false;
		_packet.session->player->angle = data.angle;
	}
}

void InGame::AckSpawnBullet( const Packet& _packet )
{
	BULLET_SHOT_INFO data = FromJson<BULLET_SHOT_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. owner:", data.owner, ",nick:", _packet.session->loginInfo.nickname );
		return;
	}

	for ( int i = 0; i < data.bullets.size(); ++i )
	{
		data.bullets[i].serial = Global::GetNewSerial();

		ActorInfo* actor = new ActorInfo();
		actor->prefab = data.prefab;
		actor->isLocal = data.isLocal;
		actor->serial = data.bullets[i].serial;
		_packet.session->stage->RegistActor( actor );
	}

	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_BULLET_ACK, data ) );
	data.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_BULLET_ACK, data ) );
}

void InGame::AckRemoveActors( const Packet& _packet )
{
	SERIALS_INFO data = FromJson<SERIALS_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( REMOVE_ACTORS_ACK, data ) );

	for ( SerialType serial : data.serials )
	{
		ActorInfo* actor = _packet.session->stage->GetActor( serial );
		_packet.session->stage->UnregistActor( actor );
		Global::Memory::SafeDelete( actor );
	}
}

void InGame::AckSyncMovement( const Packet& _packet )
{
	MOVEMENT_INFO data = FromJson<MOVEMENT_INFO>( _packet );
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

	actor->pos = data.pos;
	actor->vel = data.vel;

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_MOVEMENT_ACK, data ) );
}

void InGame::AckSyncReload( const Packet& _packet )
{
	SERIAL_INFO data = FromJson<SERIAL_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_RELOAD_ACK, data ) );
}

void InGame::AckSyncLook( const Packet& _packet )
{
	LOOK_INFO data = FromJson<LOOK_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	if ( _packet.session->player == nullptr )
	{
		Debug.LogError( "Player is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->player->angle = data.angle;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_LOOK_ANGLE_ACK, data ) );
}

void InGame::AckSyncDodgeAction( const Packet& _packet )
{
	DODGE_INFO data = FromJson<DODGE_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_DODGE_ACTION_ACK, data ) );
}

void InGame::AckSyncSwapWeapon( const Packet& _packet )
{
	INDEX_INFO data = FromJson<INDEX_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	if ( _packet.session->player == nullptr )
	{
		Debug.LogError( "Player is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}
	
	_packet.session->player->weapon = data.index;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_SWAP_WEAPON_ACK, data ) );
}

void InGame::AckHitActors( const Packet& _packet )
{
	HITS_INFO data = FromJson<HITS_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Session is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	for ( HitInfo hit : data.hits )
	{
		ActorInfo* defender = _packet.session->stage->GetActor( hit.defender );
		if ( defender == nullptr )
		{
			Debug.LogError( "defender is null. serial:", hit.defender, ", nick:", _packet.session->loginInfo.nickname );
			return;
		}

		defender->hp = hit.hp;
		if ( defender->hp <= 0 )
		{
			// 사망처리
			PlayerInfo* player = _packet.session->stage->FindPlayer( defender->serial );
			if ( player == nullptr )
			{
				_packet.session->stage->UnregistActor( defender );
				Global::Memory::SafeDelete( defender );
			}
			else if ( !player->isDead )
			{
				// Player라면 실제로 없애진 않는다
				player->isDead = true;
				++( player->death );

				PlayerInfo* attacker = _packet.session->stage->FindPlayer( hit.attacker );
				if ( attacker == nullptr )
				{
					Debug.LogError( "attacker is null. serial:", hit.attacker, ", nick:", _packet.session->loginInfo.nickname );
					return;
				}
				++( attacker->kill );
			}
		}

		if ( hit.needRelease )
		{
			ActorInfo* bullet = _packet.session->stage->GetActor( hit.bullet );
			if ( bullet == nullptr )
			{
				Debug.LogError( "Bullet is null. serial:", hit.bullet, ", nick:", _packet.session->loginInfo.nickname );
				return;
			}

			_packet.session->stage->UnregistActor( bullet );
			Global::Memory::SafeDelete( bullet );
		}

	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( HIT_ACTORS_ACK, data ) );
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

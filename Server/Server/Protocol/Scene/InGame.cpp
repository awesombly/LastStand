#include "InGame.h"
#include "Management/SessionManager.h"
#include "Management/StageManager.h"
#include "Database/Database.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,			AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,			AckExitStage );

	ProtocolSystem::Inst().Regist( SPAWN_ACTOR_REQ,			AckSpawnActor );
	ProtocolSystem::Inst().Regist( RESPAWN_ACTOR_REQ,		AckRespawnActor );
	ProtocolSystem::Inst().Regist( SPAWN_PLAYER_REQ,		AckSpawnPlayer );
	ProtocolSystem::Inst().Regist( SPAWN_BULLET_REQ,		AckSpawnBullet );
	ProtocolSystem::Inst().Regist( REMOVE_ACTORS_REQ,		AckRemoveActors );

	ProtocolSystem::Inst().Regist( SYNC_MOVEMENT_REQ,		AckSyncMovement );
	ProtocolSystem::Inst().Regist( SYNC_RELOAD_REQ,			AckSyncReload );
	ProtocolSystem::Inst().Regist( SYNC_LOOK_ANGLE_REQ,		AckSyncLook );
	ProtocolSystem::Inst().Regist( SYNC_DODGE_ACTION_REQ,	AckSyncDodgeAction );
	ProtocolSystem::Inst().Regist( SYNC_SWAP_WEAPON_REQ,	AckSyncSwapWeapon );
	ProtocolSystem::Inst().Regist( SYNC_INTERACTION_REQ,	AckSyncInteraction );
	ProtocolSystem::Inst().Regist( SYNC_EATABLE_TARGET_REQ, AckSyncEatableTarget );
	ProtocolSystem::Inst().Regist( SYNC_EATABLE_EVENT_REQ,  AckSyncEatableEvent );
	ProtocolSystem::Inst().Regist( HIT_ACTORS_REQ,			AckHitActors );

	ProtocolSystem::Inst().Regist( INIT_SCENE_ACTORS_REQ,	AckInitSceneActors );
	ProtocolSystem::Inst().Regist( INGAME_LOAD_DATA_REQ,	AckInGameLoadData );
	ProtocolSystem::Inst().Regist( UPDATE_RESULT_INFO_REQ,  AckUpdateResultData );
}

void InGame::AckChatMessage( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
		 return;

	_packet.session->stage->Broadcast( _packet );
}

void InGame::AckExitStage( const Packet& _packet )
{

	try
	{
		const STAGE_INFO& data  = FromJson<STAGE_INFO>( _packet );
		Session* session        = _packet.session;
		Stage* stage            = StageManager::Inst().Find( data.stageSerial );

		stage->Exit( session );
		if ( stage->IsExist() )
		{
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
		else
		{
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( DELETE_STAGE_INFO, stage->info ) );
			StageManager::Inst().Erase( stage );
		}

		session->Send( UPacket( EXIT_STAGE_ACK ) );
	}
	catch ( Result )
	{

	}
}

#pragma region Spawn, Remove
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
	actor->actorType = ActorType::Actor;
	_packet.session->stage->RegistActor( actor );

	_packet.session->stage->Broadcast( UPacket( SPAWN_ACTOR_ACK, data ) );
}

void InGame::AckRespawnActor( const Packet& _packet )
{
	ACTOR_INFO data = FromJson<ACTOR_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	ActorInfo* actor = _packet.session->stage->GetActor( data.serial, false );
	if ( actor == nullptr )
	{
		Debug.LogError( "Respawn Actor not found. nick:", _packet.session->loginInfo.nickname, ", serial:", data.serial );
		return;
	}
	ActorType type = actor->actorType;
	*actor = data;
	actor->actorType = type;

	_packet.session->stage->Broadcast( UPacket( RESPAWN_ACTOR_ACK, data ) );
}

void InGame::AckSpawnPlayer( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	PLAYER_INFO data = FromJson<PLAYER_INFO>( _packet );
	data.actorInfo.serial = _packet.session->serial;

	// Nickname
	if ( _packet.session->loginInfo.nickname.empty() )
	{
		_packet.session->loginInfo.nickname = std::to_string( data.actorInfo.serial );
	}
	data.nickname = _packet.session->loginInfo.nickname;

	// Init
	PlayerInfo* player = _packet.session->player;
	if ( player == nullptr )
	{
		player = new PlayerInfo( data );
		_packet.session->player = player;
		Debug.Log( "Register new player. nick:", _packet.session->loginInfo.nickname );
		_packet.session->stage->RegistActor( &player->actorInfo );
	}
	else
	{
		player->actorInfo = data.actorInfo;
	}
	player->actorInfo.actorType = ActorType::Player;
	player->isDead = false;
	player->angle = data.angle;
	player->weapon = data.weapon;

	// Ack Protocol
	player->actorInfo.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, *player ) );
	player->actorInfo.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, *player ) );
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
		actor->hp = data.hp;
		actor->actorType = ActorType::Bullet;
		_packet.session->stage->RegistActor( actor );
	}

	data.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_BULLET_ACK, data ) );
	data.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_BULLET_ACK, data ) );
}

void InGame::AckRemoveActors( const Packet& _packet )
{
	const SERIALS_INFO& data = FromJson<SERIALS_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->Broadcast( UPacket( REMOVE_ACTORS_ACK, data ) );

	for ( SerialType serial : data.serials )
	{
		ActorInfo* actor = _packet.session->stage->GetActor( serial );
		_packet.session->stage->UnregistActor( actor );
		Global::Memory::SafeDelete( actor );
	}
}
#pragma endregion

#pragma region Sync
void InGame::AckSyncMovement( const Packet& _packet )
{
	const MOVEMENT_INFO& data = FromJson<MOVEMENT_INFO>( _packet );
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
	const SERIAL_INFO& data = FromJson<SERIAL_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_RELOAD_ACK, data ) );
}

void InGame::AckSyncLook( const Packet& _packet )
{
	const LOOK_INFO& data = FromJson<LOOK_INFO>( _packet );
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
	const DODGE_INFO& data = FromJson<DODGE_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_DODGE_ACTION_ACK, data ) );
}

void InGame::AckSyncSwapWeapon( const Packet& _packet )
{
	const INDEX_INFO& data = FromJson<INDEX_INFO>( _packet );
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

void InGame::AckSyncInteraction( const Packet& _packet )
{
	const INTERACTION_INFO& data = FromJson<INTERACTION_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	ActorInfo* target = _packet.session->stage->GetActor( data.target );
	if ( target == nullptr )
	{
		Debug.LogError( "Actor is null. serial:", data.target, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	target->inter = data.angle;
	target->pos = data.pos;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_INTERACTION_ACK, data ) );
}

void InGame::AckSyncEatableTarget( const Packet& _packet )
{
	const INTERACTION_INFO& data = FromJson<INTERACTION_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_EATABLE_TARGET_ACK, data ) );
}

void InGame::AckSyncEatableEvent( const Packet& _packet )
{
	const INTERACTION_INFO& data = FromJson<INTERACTION_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	ActorInfo* eatable = _packet.session->stage->GetActor( data.serial );
	if ( eatable == nullptr )
	{
		Debug.LogError( "Eatable is null. serial:", data.serial, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	ActorInfo* target = _packet.session->stage->GetActor( data.target );
	if ( target == nullptr )
	{
		Debug.LogError( "Actor is null. serial:", data.target, ", nick:", _packet.session->loginInfo.nickname );
		return;
	}

	/// TODO: 효과 종류 늘어나면 구분 필요
	target->hp = data.angle;
	_packet.session->stage->UnregistActor( eatable );
	Global::Memory::SafeDelete( eatable );

	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_EATABLE_EVENT_ACK, data ) );
}

void InGame::AckHitActors( const Packet& _packet )
{
	HITS_INFO data = FromJson<HITS_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Session is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	bool isPlayerKill = false;
	for ( HitInfo& hit : data.hits )
	{
		// Hitable 제거
		if ( hit.needRelease )
		{
			ActorInfo* hiter = _packet.session->stage->GetActor( hit.hiter );
			if ( hiter == nullptr )
			{
				Debug.LogError( "Hitable is null. serial:", hit.hiter, ", nick:", _packet.session->loginInfo.nickname );
				continue;
			}

			_packet.session->stage->UnregistActor( hiter );
			Global::Memory::SafeDelete( hiter );
		}

		ActorInfo* defender = _packet.session->stage->GetActor( hit.defender, false );
		if ( defender == nullptr )
		{
			continue;
		}
		
		// 샷건 같은걸 맞았으면 이미 죽은 상태일 수 있다
		if ( defender->hp > 0.0f )
		{
			// Client->Server == Damage, Server->Client == Hp
			defender->hp -= hit.hp;
			if ( defender->hp <= 0.0f )
			{
				isPlayerKill = _packet.session->stage->DeadActor( defender, hit );
			}
		}
		hit.hp = defender->hp;
	}
	_packet.session->stage->Broadcast( UPacket( HIT_ACTORS_ACK, data ) );

	// 종료 목표 체크
	if ( isPlayerKill )
	{
		PlayerInfo* winner = _packet.session->stage->FindWinner();
		if ( winner == nullptr )
		{
			return;
		}

		SERIAL_INFO protocol;
		protocol.serial = winner->actorInfo.serial;
		_packet.session->stage->Broadcast( UPacket( GAME_OVER_ACK, protocol ) );

		_packet.session->stage->isValid = false;
		SessionManager::Inst().BroadcastWaitingRoom( UPacket( DELETE_STAGE_INFO, _packet.session->stage->info ) );
	}
}
#pragma endregion

void InGame::AckInitSceneActors( const Packet& _packet )
{
	ACTORS_INFO data = FromJson<ACTORS_INFO>( _packet );
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	if ( _packet.session != _packet.session->stage->host )
	{
		Debug.LogError( "Is not host. socket:", _packet.session->GetSocket(), ", nick:", _packet.session->loginInfo.nickname);
		return;
	}

	for ( ACTOR_INFO& actorInfo : data.actors )
	{
		if ( actorInfo.serial == 0 )
		{
			actorInfo.serial = Global::GetNewSerial();
		}
		ActorInfo* actor = new ActorInfo( actorInfo );
		actor->actorType = ActorType::SceneActor;
		_packet.session->stage->RegistActor( actor );
	}

	// 클라 응답은 AckInGameLoadData에서 한다
}

void InGame::AckInGameLoadData( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	// 기존에 있던 Player들 스폰
	std::list<Session*> sessions = _packet.session->stage->GetSessions();
	for ( auto session : sessions )
	{
		if ( session == nullptr )
		{
			Debug.LogError( "Session is null. " );
			continue;
		}

		if ( session == _packet.session
			|| session->player == nullptr )
		{
			continue;
		}

		_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, *session->player ) );
	}

	// 기존에 있던 Actor들 처리
	{
		ACTORS_INFO sceneActors;
		const ActorContainer& actors = _packet.session->stage->GetActors();
		for ( const auto& actorPair : actors )
		{
			if ( actorPair.second == nullptr )
			{
				Debug.LogError( "Actor is null. nick:", _packet.session->loginInfo.nickname );
				continue;
			}

			switch ( actorPair.second->actorType )
			{
			case ActorType::Actor:
			{
				_packet.session->Send( UPacket( SPAWN_ACTOR_ACK, *actorPair.second ) );
			} break;
			case ActorType::Player:
			case ActorType::Bullet:
			{
				// Player는 위에서 처리
				// Bullet은 일단 스폰 안해도 될 것 같다
			} break;
			case ActorType::SceneActor:
			{
				sceneActors.actors.push_back( *actorPair.second );

				// 너무 많으면 패킷 사이즈를 초과해 나눠보낸다
				if ( sceneActors.actors.size() >= 20 )
				{
					_packet.session->Send( UPacket( INIT_SCENE_ACTORS_ACK, sceneActors ) );
					sceneActors.actors.clear();
				}
			} break;
			case ActorType::None:
			default:
			{
				Debug.LogError( "Not processed type. type:", ( int )actorPair.second->actorType );
				throw std::exception( "# Not processed type." );
			} break;
			}
		}

		if ( sceneActors.actors.size() > 0 )
		{
			_packet.session->Send( UPacket( INIT_SCENE_ACTORS_ACK, sceneActors ) );
		}
	}
}

void InGame::AckUpdateResultData( const Packet& _packet )
{
	try
	{
		RESULT_INFO data   = FromJson<RESULT_INFO>( _packet );
		USER_DATA userData = Database::Inst().GetUserData( data.uid );

		float offset = ( float )( ( data.kill + 1 ) / ( data.death + 1 ) );
		userData.exp += ( 100.0f * offset ) * 100.0f/* 배율 */;

		float totalExp = Global::Result::GetTotalEXP( userData.level );
		while ( userData.exp >= totalExp )
		{
			userData.level += 1;
			userData.exp -= totalExp;
			totalExp = Global::Result::GetTotalEXP( userData.level );
		}

		userData.playCount += 1;
		userData.kill  += data.kill;
		userData.death += data.death;
		userData.bestKill  = userData.bestKill  < data.kill  ? data.kill  : userData.bestKill;
		userData.bestDeath = userData.bestDeath < data.death ? data.death : userData.bestDeath;

		Database::Inst().UpdateUserData( data.uid, userData );
		_packet.session->Send( UPacket( UPDATE_RESULT_INFO_ACK, userData ) );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, UPDATE_RESULT_INFO_ACK ) );
	}
}
#include "InGame.h"
#include "Management/SessionManager.h"
#include "Management/StageManager.h"
#include "Database/Database.h"

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
	ProtocolSystem::Inst().Regist( SYNC_INTERACTION_REQ,	AckSyncInteraction );
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
	Session* session = _packet.session;
	const STAGE_INFO& data  = FromJson<STAGE_INFO>( _packet );

	try
	{
		Stage* stage = StageManager::Inst().Find( data.serial );
		if ( stage == nullptr )
			 throw std::exception( "# This stage does not exist" );

		if ( !stage->Exit( session ) )
		{
			// �濡 �ƹ��� ���� ��
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

	if ( _packet.session->loginInfo.nickname.empty() )
	{
		data.nickname = std::to_string( data.actorInfo.serial );
	}
	else
	{
		data.nickname = _packet.session->loginInfo.nickname;
	}
	data.actorInfo.isLocal = true;
	_packet.session->Send( UPacket( SPAWN_PLAYER_ACK, data ) );
	data.actorInfo.isLocal = false;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SPAWN_PLAYER_ACK, data ) );

	if ( _packet.session->player == nullptr )
	{
		PlayerInfo* player = new PlayerInfo( data );
		player->actorInfo.actorType = ActorType::Player;
		_packet.session->player = player;
		_packet.session->stage->RegistActor( &player->actorInfo );
	}
	else
	{
		// ��������
		_packet.session->player->actorInfo = data.actorInfo;
		_packet.session->player->actorInfo.actorType = ActorType::Player;
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
	const INDEX_INFO& data = FromJson<INDEX_INFO>( _packet );
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

	actor->index = data.index;
	_packet.session->stage->BroadcastWithoutSelf( _packet.session, UPacket( SYNC_INTERACTION_ACK, data ) );
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
		// Hitable ����
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
			return;
		}
		
		// ���� ������ �¾����� �̹� ���� ������ �� �ִ�
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

	// ���� ��ǥ üũ
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

		Stage* stage = _packet.session->stage;
		SessionManager::Inst().BroadcastWaitingRoom( UPacket( DELETE_STAGE_INFO, stage->info ) );
		StageManager::Inst().Erase( stage );
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

	// Ŭ�� ������ AckInGameLoadData���� �Ѵ�
}

void InGame::AckInGameLoadData( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
	{
		Debug.LogError( "Stage is null. nick:", _packet.session->loginInfo.nickname );
		return;
	}

	// ������ �ִ� Player�� ����
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

	// ������ �ִ� Actor�� ó��
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
				/// TODO
			} break;
			case ActorType::Player:
			case ActorType::Bullet:
			{
				// Player�� ������ ó��
				// Bullet�� �ϴ� ���� ���ص� �� �� ����
			} break;
			case ActorType::SceneActor:
			{
				sceneActors.actors.push_back( *actorPair.second );

				// �ʹ� ������ ��Ŷ ����� �ʰ��� ����������
				if ( sceneActors.actors.size() >= 20 )
				{
					_packet.session->Send( UPacket( INIT_SCENE_ACTORS_ACK, sceneActors ) );
					sceneActors.actors.clear();
				}
			} break;
			case ActorType::None:
			default:
			{
				Debug.LogError( "Not processed type. type:", actorPair.second->actorType );
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
	RESULT_INFO data = FromJson<RESULT_INFO>( _packet );

	try
	{
		USER_DATA userData = Database::Inst().GetUserData( data.uid );

		userData.exp += 1000.0f;
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
	catch ( std::exception _error )
	{

	}
}
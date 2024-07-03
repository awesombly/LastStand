#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info ), isValid( true )
{
	Debug.Log( "Stage ", info.stageSerial, " has been created" );
	Debug.Log( "< ", _host->loginInfo.nickname, " > has entered Stage ", info.stageSerial );
	Debug.Log( "Host changed to < ", host->loginInfo.nickname, " > on stage ", info.stageSerial );

	sessions.push_back( host );
	_host->stage = this;
}

void Stage::Entry( Session* _session )
{
	if ( !isValid )
	{
		Debug.LogWarning( "The stage after the game" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	if ( sessions.size() + 1 > info.personnel.maximum )
	{
		Debug.LogWarning( "The stage is full of people" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	_session->stage = this;
	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();
	Debug.Log( "< ", _session->loginInfo.nickname, " > has entered Stage ", info.stageSerial );
}

void Stage::Exit( Session* _session )
{
	if ( sessions.size() <= 0 )
	{
		Debug.LogWarning( "There's no one in the stage" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	if ( _session->player != nullptr )
	{
		SERIALS_INFO protocol;
		protocol.serials.push_back( _session->player->actorInfo.serial );
		BroadcastWithoutSelf( _session, UPacket( REMOVE_ACTORS_ACK, protocol ) );
		UnregistActor( &_session->player->actorInfo );
		Global::Memory::SafeDelete( _session->player );
	}

	sessions.erase( std::find( sessions.begin(), sessions.end(), _session ) );
	info.personnel.current = ( int )sessions.size();
	Debug.Log( "The < ", _session->loginInfo.nickname, " > has left Stage ", info.stageSerial );

	if ( sessions.size() > 0 && host->GetSocket() == _session->GetSocket() )
	{
		 host = *sessions.begin();
		 info.hostSerial = host->serial;
		 Debug.Log( "Host changed to < ", host->loginInfo.nickname, " > on stage ", info.stageSerial );

		 SERIAL_INFO protocol;
		 protocol.serial = info.hostSerial;
		 _session->stage->BroadcastWithoutSelf( _session, UPacket( CHANGE_HOST_ACK, protocol ) );
	}

	_session->stage = nullptr;
}

void Stage::Clear()
{
	auto iter = sessions.begin();
	while ( iter != sessions.end() )
	{
		Exit( *iter );
		iter = sessions.begin();
	}
}

bool Stage::DeadActor( ActorInfo* _dead, const HitInfo& _hit )
{
	if ( _dead == nullptr )
	{
		//Debug.LogError( "Actor is null. serial:", _hit.defender );
		return false;
	}

	bool isPlayerKill = false;
	switch ( _dead->actorType )
	{
	case ActorType::Actor:
	case ActorType::Bullet:
	{
		UnregistActor( _dead );
		Global::Memory::SafeDelete( _dead );
	} break;
	case ActorType::Player:
	{
		PlayerInfo* player = FindPlayer( _dead->serial );
		if ( player == nullptr || player->isDead )
		{
			//Debug.LogWarning( "Already dead player. serial:", _hit.attacker );
			break;
		}

		// Player라면 실제로 없애진 않는다
		player->isDead = true;
		++( player->death );
		PlayerInfo* attacker = FindPlayer( _hit.attacker );
		if ( attacker == nullptr )
		{
			//Debug.LogError( "Attacker is null. serial:", _hit.attacker );
			break;
		}

		Debug.Log( "< ", player->nickname, " > Died on Stage ", info.stageSerial, "( Attacker < ", attacker->nickname, " > )" );

		++( attacker->kill );
		++info.currentKill;
		isPlayerKill = true;
	} break;
	case ActorType::SceneActor:
	{
		// 미리 배치된 Actor는 동기화 용도로 놔둔다
		PlayerInfo* attacker = FindPlayer( _hit.attacker );
		Debug.Log( "< ", attacker->nickname, " > Player destroyed object < ", _dead->serial, " >" );
	} break;
	case ActorType::None:
	default:
	{
		//Debug.LogError( "Not processed type. type:", ( int )_dead->actorType );
		throw std::exception( "Not processed type." );
	} break;
	}

	return isPlayerKill;
}

PlayerInfo* Stage::FindWinner() const
{
	if ( info.currentKill < info.targetKill )
	{
		return nullptr;
	}

	PlayerInfo* winner = nullptr;
	for ( Session* session : sessions )
	{
		if ( session == nullptr || session->player == nullptr )
		{
			continue;
		}

		if ( winner == nullptr || winner->kill < session->player->kill )
		{
			winner = session->player;
			Debug.Log( "< ", winner->nickname, " > won Stage ", info.stageSerial );
		}
	}

	return winner;
}

void Stage::RegistActor( ActorInfo* _actor )
{
	if ( _actor == nullptr )
	{
		//Debug.LogWarning( "Actor is null.(Regist)" );
		return;
	}

	if ( actors.contains( _actor->serial ) )
	{
		//Debug.LogWarning( "Already exist actor. serial:", _actor->serial );
		return;
	}

	actors[_actor->serial] = _actor;
}

void Stage::UnregistActor( const ActorInfo* _actor )
{
	if ( _actor == nullptr )
	{
		//Debug.LogWarning( "Actor is null.(Unregist)" );
		return;
	}

	if ( !actors.contains( _actor->serial ) )
	{
		//Debug.LogWarning( "Actor not found.(Unregist ) serial:", _actor->serial );
		return;
	}

	actors.erase( _actor->serial );
}

ActorInfo* Stage::GetActor( SerialType _serial, bool _useLoging ) const
{
	auto findItr = actors.find( _serial );
	if ( findItr == actors.cend() )
	{
		if ( _useLoging )
		{
			//Debug.LogWarning( "Actor not found. serial:", _serial );
		}
		return nullptr;
	}

	return findItr->second;
}

ActorContainer& Stage::GetActors()
{
	return actors;
}

std::list<Session*>& Stage::GetSessions()
{
	return sessions;
}

PlayerInfo* Stage::FindPlayer( SerialType _serial ) const
{
	for ( Session* session : sessions )
	{
		if ( session == nullptr || session->player == nullptr )
		{
			continue;
		}

		if ( session->player->actorInfo.serial == _serial )
		{
			return session->player;
		}
	}

	return nullptr;
}

void Stage::ClearActors()
{
	for ( auto actorPair : actors )
	{
		Global::Memory::SafeDelete( actorPair.second );
	}

	actors.clear();
}

void Stage::Broadcast( const UPacket& _packet ) const
{
	for ( auto iter = sessions.begin(); iter != sessions.end(); iter++ )
		( *iter )->Send( _packet );
}

void Stage::BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const
{
	for ( Session* session : sessions )
	{
		if ( session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

void Stage::Send( SOCKET _socket, const UPacket& _packet ) const
{
	for ( Session* session : sessions )
	{
		if ( session->GetSocket() == _socket )
		{
			session->Send( _packet );
			return;
		}
	}
}

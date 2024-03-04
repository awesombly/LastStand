#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info )
{
	Debug.Log( "# Stage ", info.serial, " The host has been changed< ", host->loginInfo.nickname, " >" );
	sessions.push_back( host );
	_host->stage = this;
}

bool Stage::Entry( Session* _session )
{
	if ( sessions.size() + 1 > info.personnel.maximum )
	{
		Debug.LogWarning( "# The stage is full of people" );
		return false;
	}

	_session->stage = this;
	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();

	return true;
}

bool Stage::Exit( Session* _session )
{
	if ( sessions.size() <= 0 )
		 throw std::exception( "There's no one in the stage" );

	if ( _session->player != nullptr )
	{
		SERIALS_INFO protocol;
		protocol.serials.push_back( _session->player->actorInfo.serial );
		_session->stage->BroadcastWithoutSelf( _session, UPacket( REMOVE_ACTORS_ACK, protocol ) );
		_session->stage->UnregistActor( &_session->player->actorInfo );
		Global::Memory::SafeDelete( _session->player );
	}

	sessions.erase( std::find( sessions.begin(), sessions.end(), _session ) );
	info.personnel.current = ( int )sessions.size();

	if ( sessions.size() > 0 && host->GetSocket() == _session->GetSocket() )
	{
		 host = *sessions.begin();
		 Debug.Log( "# Stage ", info.serial, " The host has been changed< ", host->loginInfo.nickname, " >" );
	}

	_session->stage = nullptr;

	return sessions.size() > 0;
}

void Stage::RegistActor( ActorInfo* _actor )
{
	if ( _actor == nullptr )
	{
		Debug.LogWarning( "Actor is null." );
		return;
	}

	if ( actors.contains( _actor->serial ) )
	{
		Debug.LogWarning( "Already exist actor. serial:", _actor->serial );
		return;
	}

	actors[_actor->serial] = _actor;
}

void Stage::UnregistActor( const ActorInfo* _actor )
{
	if ( _actor == nullptr )
	{
		Debug.LogWarning( "Actor is null." );
		return;
	}

	if ( !actors.contains( _actor->serial ) )
	{
		Debug.LogWarning( "Actor not found. serial:", _actor->serial );
		return;
	}

	actors.erase( _actor->serial );
}

ActorInfo* Stage::GetActor( SerialType _serial ) const
{
	auto findItr = actors.find( _serial );
	if ( findItr == actors.cend() )
	{
		Debug.LogWarning( "Actor not found. serial:", _serial );
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

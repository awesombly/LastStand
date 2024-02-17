#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info )
{
	Debug.Log( "# Stage ", info.serial, " The host has been changed< ", host->loginInfo.nickname, " >" );
	sessions.push_back( host );
}

bool Stage::Entry( Session* _session )
{
	if ( sessions.size() + 1 > info.personnel.maximum )
	{
		Debug.LogWarning( "# The stage is full of people" );
		return false;
	}

	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();

	return true;
}

bool Stage::Exit( Session* _session )
{
	if ( sessions.size() <= 0 )
		 throw std::exception( "There's no one in the stage" );

	sessions.erase( std::find( sessions.begin(), sessions.end(), _session ) );
	info.personnel.current = ( int )sessions.size();

	if ( sessions.size() > 0 && host->GetSocket() == _session->GetSocket() )
	{
		 host = *sessions.begin();
		 Debug.Log( "# Stage ", info.serial, " The host has been changed< ", host->loginInfo.nickname, " >" );

	}

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

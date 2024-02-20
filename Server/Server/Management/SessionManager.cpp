#include "SessionManager.h"
#include "ProtocolSystem.h"
#include "Protocol/Protocol.hpp"

SessionManager::~SessionManager()
{
	auto iter( std::begin( sessions ) );
	while ( iter++ != std::end( sessions ) )
	{
		Global::Memory::SafeDelete( ( *iter ) );
	}

	sessions.clear();
}

#pragma region Default
bool SessionManager::Initialize()
{
	std::cout << "Create thread for unresponsive session processing" << std::endl;
	std::cout << "Check for unresponsive sessions" << std::endl;
	std::thread th( [&]() { ConfirmDisconnect(); } );
	th.detach();

	return true;
}

void SessionManager::ConfirmDisconnect()
{
	while ( true )
	{
		for ( const auto& session : sessions )
		{
			if ( session == nullptr )
				 continue;

			if ( !session->CheckAlive() )
			{
				Debug.Log( "# Remove unresponsive session ( ", session->GetPort(), " ", session->GetAddress(), " )" );
				Erase( session );
			}
		}
	}
}
#pragma endregion

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	if ( _session == nullptr )
		 return;

	Debug.Log( "# Register a new session ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );

	std::lock_guard<std::mutex> lock( mtx );
	sessions.push_back( _session );
}

void SessionManager::Erase( Session* _session )
{
	if ( _session == nullptr )
		 return;

	Debug.Log( "# The session has left ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );
	
	std::lock_guard<std::mutex> lock( mtx );
	if ( _session->stage != nullptr )
		 _session->stage->Exit( _session );

	for ( std::list<Session*>::const_iterator iter = sessions.begin(); iter != sessions.end(); iter++ )
	{
		if ( ( *iter )->GetSocket() == _session->GetSocket() )
		{
			sessions.erase( iter );
			break;
		}
	}

	_session->ClosedSocket();
	Global::Memory::SafeDelete( _session );
}

void SessionManager::Broadcast( const UPacket& _packet ) const
{
	for ( const auto& session : sessions )
		  session->Send( _packet );
}

void SessionManager::BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const
{
	for ( const auto& session : sessions )
	{
		if ( session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( const UPacket& _packet )
{
	for ( const auto& session : sessions )
	{
		if ( session->stage == nullptr )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( Session* _session, const UPacket& _packet )
{
	for ( const auto& session : sessions )
	{
		if ( session->stage == nullptr && session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

const std::list<Session*>& SessionManager::GetSessions() const
{
	return sessions;
}
#pragma endregion
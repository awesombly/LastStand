#include "SessionManager.h"
#include "ProtocolSystem.h"
#include "Protocol/Protocol.hpp"
#include "StageManager.h"

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
		static std::queue<Session*> removeSessions;
		{
			std::lock_guard<std::mutex> lock( mtx );
			for ( const auto& session : sessions )
			{
				if ( session == nullptr )
					continue;

				if ( !session->CheckAlive() )
					removeSessions.push( session );
			}

			while ( !removeSessions.empty() )
			{
				Session* session = removeSessions.front();
				Debug.Log( "# Remove unresponsive session ( ", session->GetPort(), " ", session->GetAddress(), " )" );
				Debug.Log( "# The session has left ( ", session->GetPort(), " ", session->GetAddress(), " )" );
				if ( session->stage != nullptr )
				{
					Stage* stage = session->stage;
					if ( !stage->Exit( session ) )
					{
						BroadcastWaitingRoom( session, UPacket( DELETE_STAGE_INFO, stage->info ) );
						StageManager::Inst().Erase( stage );
					}
					else
					{
						BroadcastWaitingRoom( session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
					}
				}

				for ( std::list<Session*>::const_iterator iter = sessions.begin(); iter != sessions.end(); iter++ )
				{
					if ( ( *iter )->GetSocket() == session->GetSocket() )
					{
						sessions.erase( iter );
						break;
					}
				}

				session->ClosedSocket();
				Global::Memory::SafeDelete( session );
				removeSessions.pop();
			}
		}
		std::this_thread::sleep_for( std::chrono::milliseconds( 1000 ) );
	}
}
#pragma endregion

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	std::lock_guard<std::mutex> lock( mtx );
	if ( _session == nullptr )
		 return;

	Debug.Log( "# Register a new session ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );

	sessions.push_back( _session );
}

void SessionManager::Erase( Session* _session )
{
	std::lock_guard<std::mutex> lock( mtx );
	if ( _session == nullptr )
		 return;
	
	Debug.Log( "# The session has left ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );
	if ( _session->stage != nullptr )
	{
		Stage* stage = _session->stage;
		if ( !stage->Exit( _session ) )
		{
			BroadcastWaitingRoom( _session, UPacket( DELETE_STAGE_INFO, stage->info ) );
			StageManager::Inst().Erase( stage );
		}
		else
		{
			BroadcastWaitingRoom( _session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
	}

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
#include "SessionManager.h"
#include "Protocol/Protocol.hpp"

SessionManager::~SessionManager()
{
	auto pair( std::begin( sessions ) );
	while ( pair++ != std::end( sessions ) )
	{
		Global::Memory::SafeDelete( pair->second );
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
		for ( const std::pair<SOCKET, Session*>& pair : sessions )
		{
			Session* session = pair.second;
			if ( !session->CheckAlive() )
			{
				std::cout << "Remove unresponsive session( " << session->GetPort() << ", " << session->GetAddress() << " )" << std::endl;
				unresponsiveSessions.push( session );
			}
		}

		std::lock_guard<std::mutex> lock( mtx );
		{
			while ( !unresponsiveSessions.empty() )
			{
				Session* session = unresponsiveSessions.front();

				sessions.erase( session->GetSocket() );
				Global::Memory::SafeDelete( session );
				unresponsiveSessions.pop();
			}
		}
	}
}

void SessionManager::Send( const SOCKET& _socket, const UPacket& _packet ) const
{
	Session* session = Find( _socket );
	if ( session == nullptr )
	{
		std::cout << "The session was not found" << std::endl;
		return;
	}

	session->Send( _packet );
}
#pragma endregion

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	if ( _session == nullptr )
		return;

	std::cout << "Register a new session( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		sessions[_session->GetSocket()] = _session;
	}
}

void SessionManager::Erase( Session* _session )
{
	if ( _session == nullptr )
		return;

	std::cout << "The session has left( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		SOCKET socket = _session->GetSocket();
		_session->ClosedSocket();
		Global::Memory::SafeDelete( _session );
		sessions.erase( socket );
	}
}

Session* SessionManager::Find( const SOCKET& _socket ) const
{
	const auto& iter = sessions.find( _socket );
	if ( iter == std::cend( sessions ) )
		 return nullptr;

	return iter->second;
}

void SessionManager::Broadcast( const UPacket& _packet ) const
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		session->Send( _packet );
	}
}

void SessionManager::BroadcastWithoutSelf( const SOCKET& _socket, const UPacket& _packet ) const
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		if ( session->GetSocket() != _socket )
			session->Send( _packet );
	}
}

std::unordered_map<SOCKET, Session*> SessionManager::GetSessions() const
{
	return sessions;
}
#pragma endregion

#pragma region Stage Management
void SessionManager::CreateStage( const SOCKET& _host, const STAGE_INFO& _info )
{
	if ( stages.contains( _info.serial ) )
	{
		std::cout << " The stage already exists" << std::endl;
	}

	Session* host = SessionManager::Inst().Find( _host );
	std::cout << "Create a new Stage( " << host->GetPort() << ", " << host->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		stages[_info.serial] = new Stage( _host, _info );
	}
}

void SessionManager::EntryStage( const SOCKET& _session, const STAGE_INFO& _info )
{
	if ( !stages.contains( _info.serial ) )
	{
		std::cout << "This stage does not exist" << std::endl;
		return;
	}

	Session* session = Find( _session );
	std::cout << "The session has entered Stage " << _info.serial << "( " << session->GetPort() << ", " << session->GetAddress() << " )" << std::endl;
	stages[_info.serial]->Entry( session );
}

std::unordered_map<SerialType, Stage*> SessionManager::GetStages() const
{
	return stages;
}
#pragma endregion
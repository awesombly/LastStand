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

Session* SessionManager::Find( const SOCKET& _socket ) const
{
	const auto& iter = sessions.find( _socket );
	if ( iter == std::cend( sessions ) )
		 return nullptr;

	return iter->second;
}

std::unordered_map<SOCKET, Session*> SessionManager::GetSessions() const
{
	return sessions;
}

void SessionManager::Push( Session* _session )
{
	std::cout << "Resister a new session( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;

	std::lock_guard<std::mutex> lock( mtx );
	{
		sessions[_session->GetSocket()] = _session;
	}
}

void SessionManager::Erase( Session* _session )
{
	std::cout << "The session has left( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;

	std::lock_guard<std::mutex> lock( mtx );
	{
		SOCKET socket = _session->GetSocket();
		_session->ClosedSocket();
		Global::Memory::SafeDelete( _session );
		sessions.erase( socket );
	}
}
#include "SessionManager.h"
#include "../Global/Global.hpp"


SessionManager::~SessionManager()
{
	auto pair( std::begin( sessions ) );
	while ( pair++ != std::end( sessions ) )
	{
		SafeDelete( pair->second );
	}

	sessions.clear();
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

	cs.Lock();
	sessions[_session->GetSocket()] = _session;
	cs.UnLock();
}

void SessionManager::Erase( Session* _session )
{
	std::cout << "The session has left( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;

	SOCKET socket = _session->GetSocket();
	cs.Lock();
	_session->ClosedSocket();
	SafeDelete( _session );
	sessions.erase( socket );
	cs.UnLock();
}

void SessionManager::BroadCast( const Packet& _packet, const std::unordered_map<SOCKET, Session*>& _sessions )
{
	for ( const std::pair<SOCKET, Session*>& pair : _sessions )
	{
		Session* session = pair.second;
		session->Send( _packet );
	}
}

void SessionManager::BroadCast( const Packet& _packet ) const
{
	BroadCast( _packet, sessions );
}
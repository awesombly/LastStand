#include "SessionManager.h"
#include "../Global/Global.hpp"
#include "../Protocol/Protocol.h"

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

	 ConnectMessage message;
	 message.message = "Welcome Join Server!!";

	 _session->Send( UPacket( message ) );

	 _session->Send( UPacket( message ) );

	 SampleProtocol sample;
	 sample.name = "wns";
	 sample.money = 1924;
	 sample.speed = 17.6f;

	 _session->Send( UPacket( sample ) );
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
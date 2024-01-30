#include "ProtocolSystem.h"
#include "../Managed/SessionManager.h"

void ProtocolSystem::Process( const Packet& _packet )
{
	if ( !protocols.contains( _packet.type ) )
	{
		std::cout << "The protocol is not registered." << std::endl;
		return;
	}

	protocols[_packet.type]( _packet );
}

void ProtocolSystem::Regist( const IProtocol& _protocol, void( *_func )( const Packet& ) )
{
	if ( !protocols.contains( _protocol.type ) )
	{
		std::cout << "The protocol is duplicated." << std::endl;
		return;
	}

	protocols[_protocol.type] = _func;
}

void ProtocolSystem::Bind()
{
	Regist( ChatMessage(), Broadcast );
}

void ProtocolSystem::Broadcast( const Packet& _packet )
{
	for ( const std::pair<SOCKET, Session*>& pair : SessionManager::Inst().GetSessions() )
	{
		Session* session = pair.second;
		session->Send( _packet );
	}
}

void ProtocolSystem::BroadcastWithoutSelf( const Packet& _packet )
{
	for ( const std::pair<SOCKET, Session*>& pair : SessionManager::Inst().GetSessions() )
	{
		Session* session = pair.second;
		if ( session->GetSocket() != _packet.socket )
	 		 session->Send( _packet );
	}
}
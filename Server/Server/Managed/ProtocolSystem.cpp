#include "ProtocolSystem.h"
#include "Managed/SessionManager.h"
#include <cereal/cereal.hpp>
#include <cereal/archives/json.hpp>

void ProtocolSystem::Initialize()
{
	Regist( ChatMessage(),    Broadcast );
	Regist( SampleProtocol(), Sample );
	Regist( ConnectMessage(), ConnectSession );
	
	std::cout << "Function binding completed for packet processing" << std::endl;
}

void ProtocolSystem::Sample( const Packet& _packet )
{
	SampleProtocol message = FromJson<SampleProtocol>( _packet );

	std::cout << message.name << " " << message.speed << " " << message.money << std::endl;
}

void ProtocolSystem::ConnectSession( const Packet& _packet )
{
	ConnectMessage message = FromJson<ConnectMessage>( _packet );

	std::cout << message.message << std::endl;
}

void ProtocolSystem::Process( const Packet& _packet )
{
	if ( !protocols.contains( _packet.type ) )
	{
		std::cout << "The protocol is not registered" << std::endl;
		return;
	}

	protocols[_packet.type]( _packet );
}

void ProtocolSystem::Regist( const IProtocol& _protocol, void( *_func )( const Packet& ) )
{
	u_short type = _protocol.GetPacketType();
	if ( protocols.contains( type ) )
	{
		std::cout << "The protocol is duplicated" << std::endl;
		return;
	}

	protocols[type] = _func;
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
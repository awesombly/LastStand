#include "ProtocolSystem.h"
#include "SessionManager.h"
#include "Protocol/Scene/Login.h"
#include "Protocol/Scene/Lobby/Lobby.h"

ProtocolSystem::~ProtocolSystem()
{
	for ( auto iter = scenes.begin(); iter != scenes.end(); iter++ )
		  Global::Memory::SafeDelete( *iter );
}

void ProtocolSystem::Initialize()
{
	scenes.push_back( new Login() );
	scenes.push_back( new Lobby() );

	Bind();
	std::cout << "Function binding completed for packet processing" << std::endl;
}

void ProtocolSystem::Bind()
{
	Regist( PACKET_CHAT_MSG, Broadcast );
	Regist( PACKET_HEARTBEAT, []( const Packet& ) {} );

	for ( auto iter = scenes.begin(); iter != scenes.end(); iter++ )
	{
		IScene* scene = *iter;
		if ( scene != nullptr )
			 scene->Bind();
	}
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

void ProtocolSystem::Regist( const PacketType& _type, void( *_func )( const Packet& ) )
{
	if ( protocols.contains( _type ) )
	{
		std::cout << "The protocol is duplicated" << std::endl;
		return;
	}

	protocols[_type] = _func;
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
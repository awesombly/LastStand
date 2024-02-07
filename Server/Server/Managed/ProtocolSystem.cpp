#include "ProtocolSystem.h"
#include "Managed/SessionManager.h"
#include <Database/Database.h>

void ProtocolSystem::Initialize()
{
	Regist( ChatMessage(), Broadcast );
	Regist( ReqLogin(),    Login );
	
	std::cout << "Function binding completed for packet processing" << std::endl;
}

void ProtocolSystem::Login( const Packet& _packet )
{
	auto data = FromJson<ReqLogin>( _packet );
	try
	{
		UserData user = Database::Inst().Search( data.email.c_str() );
		if ( data.password.compare( user.password ) != 0 )
			 throw std::exception( "The password does not match" );

		ResLogin protocol;
		protocol.nickname = user.nickname;
		protocol.email    = user.email;
		protocol.password = user.password;

		SessionManager::Inst().Send( _packet.socket, UPacket( protocol ) );
	}
	catch ( const std::exception& _error )
	{
		// 일단 빈 객체를 보내고
		// 에러 관련 프로토콜을 정의하는 등 클라에서 처리할 수 있는 방안을 찾아야됨
		std::cout << "Exception : " << _error.what() << std::endl;
		SessionManager::Inst().Send( _packet.socket, UPacket( ResLogin() ) );
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
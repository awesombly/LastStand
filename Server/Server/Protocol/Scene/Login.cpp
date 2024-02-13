#include "Login.h"
#include "Management/SessionManager.h"
#include <Database/Database.h>

void Login::Bind()
{
	// Login, SignUp
	ProtocolSystem::Inst().Regist( CONFIRM_LOGIN_REQ,   ConfirmMatchData );
	ProtocolSystem::Inst().Regist( DUPLICATE_EMAIL_REQ, ConfirmDuplicateInfo );
	ProtocolSystem::Inst().Regist( CONFIRM_ACCOUNT_REQ, AddToDatabase );
}

void Login::ConfirmMatchData( const Packet& _packet )
{
	auto data = FromJson<LOGIN_INFO>( _packet );
	try
	{
		UserData user = Database::Inst().Search( "email", data.email );
		if ( data.password.compare( user.password ) != 0 )
			throw std::exception( "The password does not match" );

		LOGIN_INFO protocol;
		protocol.nickname = user.nickname;

		
		_packet.session->Send( UPacket( CONFIRM_LOGIN_ACK, protocol ) );
	}
	catch ( const std::exception& _error )
	{
		// 일단 빈 객체를 보내고
		// 에러 관련 프로토콜을 정의하는 등 클라에서 처리할 수 있는 방안을 찾아야됨
		std::cout << "Exception : " << _error.what() << std::endl;
		_packet.session->Send( UPacket( CONFIRM_LOGIN_ACK, LOGIN_INFO() ) );
	}
}

void Login::ConfirmDuplicateInfo( const Packet& _packet )
{
	auto data = FromJson<LOGIN_INFO>( _packet );
	CONFIRM protocol;

	try
	{
		UserData user = Database::Inst().Search( "email", data.email );
		protocol.isCompleted = false;
	}
	catch ( const std::exception& )
	{
		protocol.isCompleted = true;
	}

	_packet.session->Send( UPacket( DUPLICATE_EMAIL_ACK, protocol ) );
}

void Login::AddToDatabase( const Packet& _packet )
{
	auto data = FromJson<LOGIN_INFO>( _packet );

	CONFIRM protocol;
	protocol.isCompleted = Database::Inst().Insert( UserData{ data.nickname, data.email, data.password } );

	_packet.session->Send( UPacket( CONFIRM_ACCOUNT_ACK, protocol ) );
}
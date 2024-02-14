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
		LOGIN_INFO info = Database::Inst().Search( "email", data.email );
		if ( data.password.compare( info.password ) != 0 )
			 throw std::exception( "The password does not match" );

		std::cout << "# < " << info.nickname << " > entered the lobby" << std::endl;
		
		_packet.session->loginInfo = info;
		_packet.session->Send( UPacket( CONFIRM_LOGIN_ACK, info ) );
	}
	catch ( const std::exception& _error )
	{
		std::cout << "Exception : " << _error.what() << std::endl;
	}
}

void Login::ConfirmDuplicateInfo( const Packet& _packet )
{
	auto data = FromJson<LOGIN_INFO>( _packet );
	CONFIRM protocol;

	try
	{
		LOGIN_INFO user = Database::Inst().Search( "email", data.email );
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
	protocol.isCompleted = Database::Inst().Insert( LOGIN_INFO{ data.nickname, data.email, data.password } );

	_packet.session->Send( UPacket( CONFIRM_ACCOUNT_ACK, protocol ) );
}
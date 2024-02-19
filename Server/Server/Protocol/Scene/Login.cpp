#include "Login.h"
#include "Management/SessionManager.h"
#include <Database/Database.h>

void Login::Bind()
{
	ProtocolSystem::Inst().Regist( CONFIRM_LOGIN_REQ,   ConfirmMatchData );
	ProtocolSystem::Inst().Regist( DUPLICATE_EMAIL_REQ, ConfirmDuplicateInfo );
	ProtocolSystem::Inst().Regist( CONFIRM_ACCOUNT_REQ, AddToDatabase );
}

void Login::ConfirmMatchData( const Packet& _packet )
{
	auto data = FromJson<LOGIN_INFO>( _packet );
	Session* session = _packet.session;
	try
	{
		if ( Global::String::Trim( data.email ).empty() )
			 throw std::exception( "# The email is empty" );

		LOGIN_INFO info = Database::Inst().Search( "email", data.email );
		if ( data.email.compare( info.email ) != 0 || data.password.compare( info.password ) != 0 )
			 throw std::exception( "# Login information does not match" );

		Debug.Log( "# < ", info.nickname, " > login completed" );
		
		session->loginInfo = info;
		session->Send( UPacket( CONFIRM_LOGIN_ACK, info ) );
	}
	catch ( const std::exception& _error )
	{
		Debug.LogWarning( "# DB Exception < ", _error.what(), " >" );
		session->Send( UPacket( CONFIRM_LOGIN_ACK, LOGIN_INFO() ) );
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
	Session* session = _packet.session;
	
	CONFIRM confirm;
	try
	{
		if ( Global::String::Trim( data.nickname ).empty() )
			 throw std::exception( "# The email is empty" );

		confirm.isCompleted = Database::Inst().Insert( LOGIN_INFO{ data.nickname, data.email, data.password } );
	}
	catch ( const std::exception& _error )
	{
		confirm.isCompleted = false;
		Debug.LogError( "# DB Exception < ", _error.what(), " >" );
	}

	session->Send( UPacket( CONFIRM_ACCOUNT_ACK, confirm ) );
}
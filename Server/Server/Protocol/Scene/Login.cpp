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
	LOGIN_DATA data = FromJson<LOGIN_DATA>( _packet );
	Session* session = _packet.session;
	try
	{
		if ( Global::String::Trim( data.email ).empty() )
		{
			Debug.LogWarning( "# The email is empty" );
			throw Result::DB_ERR_INVALID_DATA;
		}

		LOGIN_DATA info = Database::Inst().GetLoginData( data.email );
		if ( data.email.compare( info.email ) != 0 || data.password.compare( info.password ) != 0 )
		{
			Debug.LogWarning( "# Login information does not match" );
			throw Result::DB_ERR_INVALID_DATA;
		}

		ACCOUNT_INFO ret;
		ret.result    = Result::OK;
		ret.loginInfo = session->loginInfo = info;
		ret.userInfo  = Database::Inst().GetUserData( info.uid );
		session->Send( UPacket( CONFIRM_LOGIN_ACK, ret ) );

		Debug.Log( "# < ", info.nickname, " > login completed" );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, CONFIRM_LOGIN_ACK ) );
		Debug.LogError( "# DB Exception < ", Error::String( _error ), " >" );
	}
}

void Login::ConfirmDuplicateInfo( const Packet& _packet )
{
	try
	{
		const auto& data = FromJson<LOGIN_DATA>( _packet );
		if ( Database::Inst().ExistLoginData( data ) )
			 throw Result::DB_ERR_DUPLICATE_DATA;
		
		_packet.session->Send( UPacket( DUPLICATE_EMAIL_ACK ) );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, DUPLICATE_EMAIL_ACK ) );
		Debug.LogError( "# DB Exception < ", Error::String( _error ), " >" );
	}
}

void Login::AddToDatabase( const Packet& _packet )
{
	try
	{
		auto data = FromJson<LOGIN_DATA>( _packet );
		if ( Global::String::Trim( data.nickname ).empty() )
			 throw Result::DB_ERR_INVALID_DATA;

		Database::Inst().CreateUserData( data.nickname, data.email, data.password );
		_packet.session->Send( UPacket( CONFIRM_ACCOUNT_ACK ) );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, CONFIRM_ACCOUNT_ACK ) );
		Debug.LogError( "# DB Exception < ", Error::String( _error ), " >" );
	}
}
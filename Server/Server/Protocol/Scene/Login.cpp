#include "Login.h"
#include "Managed/SessionManager.h"
#include <Database/Database.h>

void Login::Bind()
{
	// Login, SignUp
	ProtocolSystem::Inst().Regist( CONFIRM_SIGNUP_REQ,  ConfirmLogin );
	ProtocolSystem::Inst().Regist( DUPLICATE_EMAIL_REQ, RequestSignUpMail );
	ProtocolSystem::Inst().Regist( CONFIRM_SIGNUP_REQ,  RequestSignUp );
}

void Login::ConfirmLogin( const Packet& _packet )
{
	auto data = FromJson<ReqLogin>( _packet );
	try
	{
		UserData user = Database::Inst().Search( "email", data.email );
		if ( data.password.compare( user.password ) != 0 )
			throw std::exception( "The password does not match" );

		ResLogin protocol;
		protocol.nickname = user.nickname;

		SessionManager::Inst().Send( _packet.socket, UPacket( CONFIRM_LOGIN_RES, protocol ) );
	}
	catch ( const std::exception& _error )
	{
		// �ϴ� �� ��ü�� ������
		// ���� ���� ���������� �����ϴ� �� Ŭ�󿡼� ó���� �� �ִ� ����� ã�ƾߵ�
		std::cout << "Exception : " << _error.what() << std::endl;
		SessionManager::Inst().Send( _packet.socket, UPacket( CONFIRM_LOGIN_RES, ResLogin() ) );
	}
}

void Login::RequestSignUpMail( const Packet& _packet )
{
	auto data = FromJson<ReqSignUpMail>( _packet );
	ResSignUpMail protocol;

	try
	{
		UserData user = Database::Inst().Search( "email", data.email );
		protocol.isPossible = false;
	}
	catch ( const std::exception& )
	{
		protocol.isPossible = true;
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( DUPLICATE_EMAIL_RES, protocol ) );
}

void Login::RequestSignUp( const Packet& _packet )
{
	auto data = FromJson<ReqSignUp>( _packet );
	ResSignUp protocol;
	protocol.isCompleted = Database::Inst().Insert( UserData{ data.nickname, data.email, data.password } );

	SessionManager::Inst().Send( _packet.socket, UPacket( CONFIRM_SIGNUP_RES, protocol ) );
}
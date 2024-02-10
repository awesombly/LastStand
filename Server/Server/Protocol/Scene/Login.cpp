#include "Login.h"
#include "Managed/SessionManager.h"
#include <Database/Database.h>

void Login::Bind()
{
	// Login, SignUp
	ProtocolSystem::Inst().Regist( ReqLogin(),      ConfirmLogin );
	ProtocolSystem::Inst().Regist( ReqSignUpMail(), RequestSignUpMail );
	ProtocolSystem::Inst().Regist( ReqSignUp(),     RequestSignUp );
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

	SessionManager::Inst().Send( _packet.socket, UPacket( protocol ) );
}

void Login::RequestSignUp( const Packet& _packet )
{
	auto data = FromJson<ReqSignUp>( _packet );
	ResSignUp protocol;
	protocol.isCompleted = Database::Inst().Insert( UserData{ data.nickname, data.email, data.password } );

	SessionManager::Inst().Send( _packet.socket, UPacket( protocol ) );
}
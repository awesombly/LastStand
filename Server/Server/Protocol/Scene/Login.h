#pragma once
#include "Managed/ProtocolSystem.h"

class Login : public IScene
{
public:
	Login()          = default;
	virtual ~Login() = default;

public:
	virtual void Bind() override;

private:
	static void ConfirmLogin( const Packet& _packet );
	static void RequestSignUp( const Packet& _packet );
	static void RequestSignUpMail( const Packet& _packet );
};

// Request 서버로 보내는 요청
struct ReqLogin
{
public:
	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
};

struct ReqSignUpMail
{
public:

	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
};

struct ReqSignUp
{
public:
	std::string nickname;
	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
};

// Response 요청에 대한 서버의 답변
struct ResLogin
{
public:
	std::string nickname;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( nickname ) );
	}
};

struct ResSignUpMail
{
public:
	bool isPossible;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isPossible ) );
	}
};

struct ResSignUp
{
public:
	bool isCompleted;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isCompleted ) );
	}
};
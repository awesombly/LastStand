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

// Request ������ ������ ��û
struct ReqLogin : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
};

struct ReqSignUpMail : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
};

struct ReqSignUp : public IProtocol
{
public:
	CONSTRUCTOR()

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

// Response ��û�� ���� ������ �亯
struct ResLogin : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string nickname;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( nickname ) );
	}
};

struct ResSignUpMail : public IProtocol
{
public:
	CONSTRUCTOR()

	bool isPossible;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isPossible ) );
	}
};

struct ResSignUp : public IProtocol
{
public:
	CONSTRUCTOR()

	bool isCompleted;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isCompleted ) );
	}
};
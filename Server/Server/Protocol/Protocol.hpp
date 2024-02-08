#pragma once
#include "Global/Header.h"

#define CONSTRUCTOR() u_short GetPacketType() const override          \
{																	  \
	std::string name = typeid( *this ).name();						  \
	size_t pos = name.find_first_of( " " );							  \
																	  \
	if ( pos != std::string::npos )									  \
		 name = name.substr( pos + 1, name.length() );				  \
																	  \
	unsigned int hash = 0;											  \
	const size_t size = ::strlen( name.c_str() );					  \
	for ( size_t i = 0; i < size; i++ )								  \
	{																  \
		hash = name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;		  \
	}																  \
																	  \
	return ( u_short )hash;											  \
}																	  \

interface IProtocol
{
public:
	virtual u_short GetPacketType() const = 0;
	//
	// # Protocol ����
	// 
	// 1. CONSTRUCTOR ����
	//    - ����ü �̸����� Protocol Type�� �����ϴ� ���� �߿�
	//    - �Լ��� ����Ǵ� ����ü �ȿ��� typeid�� ����ؾ�
	//    - ���ϴ� ����� ���� �� �ִ�.

	// 2. serialize ����
	//    - Cereal ����ȭ�� ���� ���۾�����
	//    - SampleProtocol serailize �Լ��� �����Ѵ�.

	// 3. ������ serealize �Լ� �� ���� ���� ����
	//    - Json Ÿ������ �ۼ����Ͽ� ����� ������
	//    - ��Ŷ�� �������� �� ��Ʈ��ũ �۾���
	//    - ���� �� Ŭ�� ���� ������ ��ǥ�� �Ѵ�.
	
	// # ������ serealize�� ����� ������ �������� ����ȭ�Ѵ�.
};

struct SampleProtocol : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string name = "";
	float speed = 0;
	int money = 0;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( name ) );
		ar( CEREAL_NVP( speed ) );
		ar( CEREAL_NVP( money ) );
	}
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

// Both
struct ChatMessage : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};

struct ConnectMessage : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};
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
	// # Protocol 생성
	// 
	// 1. CONSTRUCTOR 정의
	//    - 구조체 이름으로 Protocol Type을 생성하는 과정 중에
	//    - 함수가 실행되는 구조체 안에서 typeid를 사용해야
	//    - 원하는 결과를 얻을 수 있다.

	// 2. serialize 정의
	//    - Cereal 직렬화를 위한 밑작업으로
	//    - SampleProtocol serailize 함수를 참고한다.

	// 3. 변수와 serealize 함수 내 선언 순서 통일
	//    - Json 타입으로 송수신하여 결과는 같지만
	//    - 패킷과 프로토콜 등 네트워크 작업은
	//    - 서버 및 클라 간의 통일을 목표로 한다.
	
	// # 서버는 serealize에 선언된 순서를 기준으로 직렬화한다.
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

// Both
struct Heartbeat : public IProtocol
{
public:
	CONSTRUCTOR()

	// 클라 연결을 확인하기 위한 프로토콜
	template <class Archive>
	void serialize( Archive& ar ) {	}
};

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


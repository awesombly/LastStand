#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT, // 주기적인 통신을 위한 프로토콜
	PACKET_CHAT_MSG,  // 일반적인 채팅 메세지

	CONFIRM_LOGIN_REQ = 1000, // 로그인 확인 요청
	CONFIRM_LOGIN_RES,        // 로그인 확인 응답
	DUPLICATE_EMAIL_REQ,      // 이메일 중복확인 요청
	DUPLICATE_EMAIL_RES,      // 이메일 중복확인 응답
	CONFIRM_SIGNUP_REQ,       // 이메일 생성 요청
	CONFIRM_SIGNUP_RES,       // 이메일 생성 응답

	CREATE_ROOM_REQ = 2000, // 방 생성 요청
	CREATE_ROOM_RES,        // 방 생성 응답
	TAKE_ROOM_LIST,         // 방 목록 전달
};


struct SampleProtocol
{
public:
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
struct Heartbeat
{
public:
	// 클라 연결을 확인하기 위한 프로토콜
	template <class Archive>
	void serialize( Archive& ar ) {	}
};

struct ChatMessage
{
public:
	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};


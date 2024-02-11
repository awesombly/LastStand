#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // 주기적인 통신을 위한 프로토콜
	PACKET_CHAT_MSG,               // 일반적인 채팅 메세지
							       
	CONFIRM_LOGIN_REQ = 1000,      // 로그인 확인 요청
	CONFIRM_LOGIN_RES,             // 로그인 확인 응답
	DUPLICATE_EMAIL_REQ,           // 이메일 중복확인 요청
	DUPLICATE_EMAIL_RES,           // 이메일 중복확인 응답
	CONFIRM_SIGNUP_REQ,            // 이메일 생성 요청
	CONFIRM_SIGNUP_RES,            // 이메일 생성 응답
							       
	CREATE_ROOM_REQ = 2000,        // 방 생성 요청
	CREATE_ROOM_RES,               // 방 생성 응답
	TAKE_ROOM_LIST,                // 방 목록 전달
};

typedef struct EmptyProtocol
{
public:
	template <class Archive>
	void serialize( Archive& ar ) {	}
} EMPTY;

typedef struct SingleString
{
public:
	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
} MESSAGE;

typedef struct SingleBoolean
{
public:
	bool isCompleted;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isCompleted ) );
	}
} CONFIRM, CHECK;

typedef struct LoginInfo
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
} LOGIN_INFO;

struct Personnel
{
public:
	int current, maximum;
	
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( current ) );
		ar( CEREAL_NVP( maximum ) );
	}
};
typedef struct RoomInfo
{
public:
	u_short uid;
	std::string title;
	Personnel personnel;

public:
	RoomInfo() = default;
	RoomInfo( u_short _uid, const std::string& _title, Personnel _personnel ) :
		      uid( _uid ), title( _title ), personnel( _personnel ) { }

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( personnel ) );
	}
} ROOM_INFO;
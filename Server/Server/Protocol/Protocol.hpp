#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // 주기적인 통신을 위한 패킷
	PACKET_CHAT_MSG,               // 채팅 메세지
							       
	CONFIRM_LOGIN_REQ = 1000,      // 로그인 요청
	CONFIRM_LOGIN_ACK,             // 로그인 응답
	CONFIRM_ACCOUNT_REQ,           // 계정 생성 요청
	CONFIRM_ACCOUNT_ACK,           // 계정 생성 응답
	DUPLICATE_EMAIL_REQ,           // 이메일 중복확인 요청
	DUPLICATE_EMAIL_ACK,           // 이메일 중복확인 응답
							       
	LOBBY_INFO_REQ = 2000,         // 로비 정보 요청
	LOBBY_INFO_ACK,                // 로비 정보 응답

	CREATE_STAGE_REQ = 3000,       // 방 생성 요청
	CREATE_STAGE_ACK,              // 방 생성 응답
	UPDATE_STAGE_INFO,             // 방 정보가 갱신됨
	INSERT_STAGE_INFO,             // 방 정보가 추가됨
	DELETE_STAGE_INFO,             // 방 정보가 삭제됨
	ENTRY_STAGE_REQ,               // 방 입장 요청
	ENTRY_STAGE_ACK,               // 방 입장 응답
	EXIT_STAGE_REQ,                // 방 퇴장 요청
	EXIT_STAGE_ACK,                // 방 퇴장 응답

	SPAWN_ACTOR_REQ = 5000,       // Actor 스폰 요청
	SPAWN_ACTOR_ACK,              // Actor 스폰 응답
};

typedef struct Vector2
{
public:
	float x, y;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
	}
} VECTOR2;

typedef struct Vector3
{
public:
	float x, y, z;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
		ar( CEREAL_NVP( z ) );
	}
} VECTOR3;

typedef struct Vector4
{
public:
	float x, y, z, w;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
		ar( CEREAL_NVP( z ) );
		ar( CEREAL_NVP( w ) );
	}
} Quaternion, QUATERNION, Quaternion;

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
	
	Personnel() : current( 0 ), maximum( 0 ) { }
	Personnel( int _cur, int _max ) : current( _cur ), maximum( _max ) { }

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( current ) );
		ar( CEREAL_NVP( maximum ) );
	}
};
typedef struct StageInfo
{
public:
	u_short uid;
	std::string title;
	Personnel personnel;

public:
	StageInfo() = default;
	StageInfo( u_short _uid, const std::string& _title, Personnel _personnel ) :
		      uid( _uid ), title( _title ), personnel( _personnel ) { }

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;

typedef struct LobbyInfo
{
public:
	std::list<STAGE_INFO> infos;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( infos ) );
	}
} LOBBY_INFO;

typedef struct ActorInfo
{
public:
	int prefab;
	SerialType serial;
	Vector3 position;
	Vector4 rotation;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( prefab ) );
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( position ) );
		ar( CEREAL_NVP( rotation ) );
	}
} ACTOR_INFO;
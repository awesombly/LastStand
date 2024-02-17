#pragma once
#include "Global/Header.h"
#include "magic_enum.hpp"

// 서버에서 패킷타입을 문자열로 출력하기위해
// 타입은 0부터 순서대로 지정되도록 합니다.
enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // 주기적인 통신을 위한 패킷
	PACKET_CHAT_MSG,               // 채팅 메세지

	// Login
	CONFIRM_LOGIN_REQ,             // 로그인 요청
	CONFIRM_LOGIN_ACK,             // 로그인 응답
	CONFIRM_ACCOUNT_REQ,           // 계정 생성 요청
	CONFIRM_ACCOUNT_ACK,           // 계정 생성 응답
	DUPLICATE_EMAIL_REQ,           // 이메일 중복확인 요청
	DUPLICATE_EMAIL_ACK,           // 이메일 중복확인 응답

	// Stage
	STAGE_INFO_REQ,                // 방 정보 요청
	STAGE_INFO_ACK,                // 방 정보 응답
	CREATE_STAGE_REQ,	           // 방 생성 요청
	CREATE_STAGE_ACK,              // 방 생성 응답
	UPDATE_STAGE_INFO,             // 방 정보가 갱신됨
	INSERT_STAGE_INFO,             // 방 정보가 추가됨
	DELETE_STAGE_INFO,             // 방 정보가 삭제됨
	ENTRY_STAGE_REQ,               // 방 입장 요청
	ENTRY_STAGE_ACK,               // 방 입장 응답
	EXIT_STAGE_REQ,                // 방 퇴장 요청
	EXIT_STAGE_ACK,                // 방 퇴장 응답

	// Actor
	SPAWN_ACTOR_REQ,               // Actor 스폰 요청
	SPAWN_ACTOR_ACK,               // Actor 스폰 응답
	SPAWN_PLAYER_ACK,              // Player 스폰 응답
	SPAWN_BULLET_REQ,              // Bullet 스폰 요청
	SPAWN_BULLET_ACK,              // Bullet 스폰 응답
	REMOVE_ACTOR_REQ,              // Actor 제거 요청
	REMOVE_ACTOR_ACK,              // Actor 제거 응답
	SYNK_MOVEMENT_REQ,             // Actor 이동 동기화 요청
	SYNK_MOVEMENT_ACK,             // Actor 이동 동기화 응답
	SYNK_RELOAD_REQ,               // 재장전 동기화 요청
	SYNK_RELOAD_ACK,               // 재장전 동기화 응답
	SYNK_LOOK_REQ,                 // Player 시선 동기화 요청
	SYNK_LOOK_ACK,                 // Player 시선 동기화 응답
	HIT_ACTOR_REQ,                 // 피격 동기화 요청
	HIT_ACTOR_ACK,                 // 피격 동기화 응답
	INGAME_LOAD_DATA_REQ,          // InGame 입장시 데이터 요청
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
} Quaternion, QUATERNION;

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

typedef struct SingleSerialType
{
public:
	SerialType serial;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
	}
} SERIAL_INFO;

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
	SerialType serial;
	std::string title;
	Personnel personnel;

public:
	StageInfo() = default;
	StageInfo( u_short _serial, const std::string& _title, Personnel _personnel ) :
		serial( _serial ), title( _title ), personnel( _personnel ) { }

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;

typedef struct ActorInfo
{
public:
	int prefab;
	bool isLocal;
	SerialType serial;
	Vector3 position;
	Vector4 rotation;
	Vector3 velocity;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( prefab ) );
		ar( CEREAL_NVP( isLocal ) );
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( position ) );
		ar( CEREAL_NVP( rotation ) );
		ar( CEREAL_NVP( velocity ) );
	}
} ACTOR_INFO;

typedef struct PlayerInfo
{
public:
	ACTOR_INFO actorInfo;
	std::string nickname;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( actorInfo ) );
		ar( CEREAL_NVP( nickname ) );
	}
} PLAYER_INFO;

typedef struct BulletInfo
{
public:
	ACTOR_INFO actorInfo;
	SerialType owner;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( actorInfo ) );
		ar( CEREAL_NVP( owner ) );
	}
} BULLET_INFO;

typedef struct HitInfo
{
public:
	bool needRelease;
	SerialType bullet;
	SerialType attacker;
	SerialType defender;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( needRelease ) );
		ar( CEREAL_NVP( bullet ) );
		ar( CEREAL_NVP( attacker ) );
		ar( CEREAL_NVP( defender ) );
	}
} HIT_INFO;

typedef struct LookInfo
{
public:
	SerialType serial;
	float angle;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( angle ) );
	}
} LOOK_INFO;

typedef struct ChatMessage
{
public:
	std::string nickname;
	std::string message;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( message ) );
	}
} CHAT_MESSAGE;
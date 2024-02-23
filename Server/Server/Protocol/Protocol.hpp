#pragma once
#include "Global/Header.h"
#include "magic_enum.hpp"

// �������� ��ŶŸ���� ���ڿ��� ����ϱ�����
// Ÿ���� 0���� ������� �����ǵ��� �մϴ�.

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // �ֱ����� ����� ���� ��Ŷ
	PACKET_CHAT_MSG,               // ä�� �޼���

	// Login
	CONFIRM_LOGIN_REQ,             // �α��� ��û
	CONFIRM_LOGIN_ACK,             // �α��� ����
	CONFIRM_ACCOUNT_REQ,           // ���� ���� ��û
	CONFIRM_ACCOUNT_ACK,           // ���� ���� ����
	DUPLICATE_EMAIL_REQ,           // �̸��� �ߺ�Ȯ�� ��û
	DUPLICATE_EMAIL_ACK,           // �̸��� �ߺ�Ȯ�� ����

	// Stage
	STAGE_INFO_REQ,                // �� ���� ��û
	STAGE_INFO_ACK,                // �� ���� ����
	CREATE_STAGE_REQ,	           // �� ���� ��û
	CREATE_STAGE_ACK,              // �� ���� ����
	UPDATE_STAGE_INFO,             // �� ������ ���ŵ�
	INSERT_STAGE_INFO,             // �� ������ �߰���
	DELETE_STAGE_INFO,             // �� ������ ������
	ENTRY_STAGE_REQ,               // �� ���� ��û
	ENTRY_STAGE_ACK,               // �� ���� ����
	EXIT_STAGE_REQ,                // �� ���� ��û
	EXIT_STAGE_ACK,                // �� ���� ����

	// Actor
	SPAWN_ACTOR_REQ,               // Actor ���� ��û
	SPAWN_ACTOR_ACK,               // Actor ���� ����
	SPAWN_PLAYER_ACK,              // Player ���� ����
	SPAWN_BULLET_REQ,              // Bullet ���� ��û
	SPAWN_BULLET_ACK,              // Bullet ���� ����
	REMOVE_ACTOR_REQ,              // Actor ���� ��û
	REMOVE_ACTOR_ACK,              // Actor ���� ����

	SYNC_MOVEMENT_REQ,             // Actor �̵� ����ȭ ��û
	SYNC_MOVEMENT_ACK,             // Actor �̵� ����ȭ ����
	SYNC_RELOAD_REQ,               // ������ ����ȭ ��û
	SYNC_RELOAD_ACK,               // ������ ����ȭ ����
	SYNC_LOOK_ANGLE_REQ,           // Player �ü� ����ȭ ��û
	SYNC_LOOK_ANGLE_ACK,           // Player �ü� ����ȭ ����
	SYNC_DODGE_ACTION_REQ,         // Player ȸ�� ����ȭ ��û
	SYNC_DODGE_ACTION_ACK,         // Player ȸ�� ����ȭ ����
	SYNC_SWAP_WEAPON_REQ,          // Player ���� ��ü ����ȭ ��û
	SYNC_SWAP_WEAPON_ACK,          // Player ���� ��ü ����ȭ ����
	HIT_ACTOR_REQ,                 // �ǰ� ����ȭ ��û
	HIT_ACTOR_ACK,                 // �ǰ� ����ȭ ����
	INGAME_LOAD_DATA_REQ,          // InGame ����� ������ ��û
};

typedef struct Empty
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
typedef struct SerialIntType
{
public:
	SerialType serial;
	int index;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( index ) );
	}
} INDEX_INFO;
typedef struct SerialFloatType
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
	int targetKill;
	Personnel personnel;

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( targetKill ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;
typedef struct ActorInfo
{
public:
	int prefab;
	bool isLocal;
	SerialType serial;
	Vector2 pos;
	Vector2 vel;
	float hp;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( prefab ) );
		ar( CEREAL_NVP( isLocal ) );
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( vel ) );
		ar( CEREAL_NVP( hp ) );
	}
} ACTOR_INFO;
typedef struct PlayerInfo
{
public:
	ACTOR_INFO actorInfo;
	std::string nickname;
	float angle;
	int weapon;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( actorInfo ) );
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( angle ) );
		ar( CEREAL_NVP( weapon ) );
	}
} PLAYER_INFO;
typedef struct BulletInfo
{
public:
	SerialType serial;
	float angle;
	float rate;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( angle ) );
		ar( CEREAL_NVP( rate ) );
	}
} BULLET_INFO;
typedef struct BulletShotInfo
{
public:
	int prefab;
	bool isLocal;
	SerialType owner;
	VECTOR2 pos;
	float look;
	float damage;
	std::vector<BULLET_INFO> bullets;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( prefab ) );
		ar( CEREAL_NVP( isLocal ) );
		ar( CEREAL_NVP( owner ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( look ) );
		ar( CEREAL_NVP( damage ) );
		ar( CEREAL_NVP( bullets ) );
	}
} BULLET_SHOT_INFO;
typedef struct HitInfo
{
public:
	bool needRelease;
	SerialType bullet;
	SerialType attacker;
	SerialType defender;
	float hp;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( needRelease ) );
		ar( CEREAL_NVP( bullet ) );
		ar( CEREAL_NVP( attacker ) );
		ar( CEREAL_NVP( defender ) );
		ar( CEREAL_NVP( hp ) );
	}
} HIT_INFO;
typedef struct MovementInfo
{
public:
	SerialType serial;
	Vector2 pos;
	Vector2 vel;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( vel ) );
	}
} MOVEMENT_INFO;
typedef struct DodgeInfo
{
public:
	SerialType serial;
	bool useCollision;
	Vector2 pos;
	Vector2 dir;
	float dur;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( useCollision ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( dir ) );
		ar( CEREAL_NVP( dur ) );
	}
} DODGE_INFO;
typedef struct ChatMessage
{
public:
	u_int serial;
	std::string nickname;
	std::string message;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( message ) );
	}
} CHAT_MESSAGE;
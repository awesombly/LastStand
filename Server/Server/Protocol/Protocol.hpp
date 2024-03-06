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
	SPAWN_PLAYER_REQ,              // Player ���� ��û
	SPAWN_PLAYER_ACK,              // Player ���� ����
	SPAWN_BULLET_REQ,              // Bullet ���� ��û
	SPAWN_BULLET_ACK,              // Bullet ���� ����
	REMOVE_ACTORS_REQ,             // Actor�� ���� ��û
	REMOVE_ACTORS_ACK,             // Actor�� ���� ����

	SYNC_MOVEMENT_REQ,             // Actor �̵� ����ȭ ��û
	SYNC_MOVEMENT_ACK,             // Actor �̵� ����ȭ ����
	SYNC_RELOAD_REQ,               // Player ������ ����ȭ ��û
	SYNC_RELOAD_ACK,               // Player ������ ����ȭ ����
	SYNC_LOOK_ANGLE_REQ,           // Player �ü� ����ȭ ��û
	SYNC_LOOK_ANGLE_ACK,           // Player �ü� ����ȭ ����
	SYNC_DODGE_ACTION_REQ,         // Player ȸ�� ����ȭ ��û
	SYNC_DODGE_ACTION_ACK,         // Player ȸ�� ����ȭ ����
	SYNC_SWAP_WEAPON_REQ,          // Player ���� ��ü ����ȭ ��û
	SYNC_SWAP_WEAPON_ACK,          // Player ���� ��ü ����ȭ ����
	SYNC_INTERACTION_REQ,          // InteractableActor ��ȣ�ۿ� ��û
	SYNC_INTERACTION_ACK,          // InteractableActor ��ȣ�ۿ� ����
	HIT_ACTORS_REQ,                // �ǰݵ� Actor�� ����ȭ ��û
	HIT_ACTORS_ACK,                // �ǰݵ� Actor�� ����ȭ ����

	INIT_SCENE_ACTORS_REQ,         // ���� ��ġ�� Actor�� �ʱ�ȭ ��û
	INIT_SCENE_ACTORS_ACK,         // ���� ��ġ�� Actor�� �ʱ�ȭ ����
	INGAME_LOAD_DATA_REQ,          // InGame ����� ������ ��û
	GAME_OVER_ACK,                 // ���� ���� ����
	UPDATE_RESULT_INFO_REQ,        // ���� ��� ���� ��û
	UPDATE_RESULT_INFO_ACK,        // ���� ��� ���� ����
};

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
} CHECK;
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
typedef struct SerialsType
{
public:
	std::vector<SerialType> serials;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serials ) );
	}
} SERIALS_INFO;
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

#pragma region Database
typedef struct LoginInfo
{
public:
	int uid;
	std::string nickname;
	std::string email;
	std::string password;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( email ) );
		ar( CEREAL_NVP( password ) );
	}
} LOGIN_INFO, LoginData, LOGIN_DATA;

typedef struct UserInfo
{
public:
	int level;
	float exp;
	int playCount;
	int kill, death;
	int bestKill, bestDeath;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( level ) );
		ar( CEREAL_NVP( exp ) );
		ar( CEREAL_NVP( playCount ) );
		ar( CEREAL_NVP( kill ) );
		ar( CEREAL_NVP( death ) );
		ar( CEREAL_NVP( bestKill ) );
		ar( CEREAL_NVP( bestDeath ) );
	}
} USER_INFO, UserData, USER_DATA;

typedef struct ResultInfo
{
	int uid;
	int kill, death;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( kill ) );
		ar( CEREAL_NVP( death ) );
	}
} RESULT_INFO, ResultData, RESULT_DATA;

typedef struct AccountInfo
{
public:
	Result result;
	LOGIN_INFO loginInfo;
	USER_INFO userInfo;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( result ) );
		ar( CEREAL_NVP( loginInfo ) );
		ar( CEREAL_NVP( userInfo ) );
	}
} ACCOUNT_INFO;
#pragma endregion

#pragma region Lobby Infomation
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
	int currentKill;
	Personnel personnel;

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( targetKill ) );
		ar( CEREAL_NVP( currentKill ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;
#pragma endregion

#pragma region Actor
enum ActorType : u_short
{ 
	None, Player, Bullet, SceneActor, Actor,
};
typedef struct ActorInfo
{
public:
	int prefab;
	bool isLocal;
	SerialType serial;
	Vector2 pos;
	Vector2 vel;
	float hp;
	float inter;

	// ���������� ���
	ActorType actorType;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( prefab ) );
		ar( CEREAL_NVP( isLocal ) );
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( vel ) );
		ar( CEREAL_NVP( hp ) );
		ar( CEREAL_NVP( inter ) );
	}
} ACTOR_INFO;
typedef struct ActorsInfo
{
public:
	std::vector<ACTOR_INFO> actors;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( actors ) );
	}
} ACTORS_INFO;
typedef struct PlayerInfo
{
public:
	ACTOR_INFO actorInfo;
	std::string nickname;
	bool isDead;
	float angle;
	int weapon;
	int kill;
	int death;

	enum class PlayerType
	{
		None = 0, Pilot, Hunter, Convict,
	};
	PlayerType type;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( actorInfo ) );
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( isDead ) );
		ar( CEREAL_NVP( angle ) );
		ar( CEREAL_NVP( weapon ) );
		ar( CEREAL_NVP( kill ) );
		ar( CEREAL_NVP( death ) );
		ar( CEREAL_NVP( type ) );
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
	float hp;
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
		ar( CEREAL_NVP( hp ) );
		ar( CEREAL_NVP( bullets ) );
	}
} BULLET_SHOT_INFO;
typedef struct HitInfo
{
public:
	bool needRelease;
	SerialType hiter;
	SerialType attacker;
	SerialType defender;
	Vector2 pos;
	float hp;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( needRelease ) );
		ar( CEREAL_NVP( hiter ) );
		ar( CEREAL_NVP( attacker ) );
		ar( CEREAL_NVP( defender ) );
		ar( CEREAL_NVP( pos ) );
		ar( CEREAL_NVP( hp ) );
	}
} HIT_INFO;
typedef struct HitsInfo
{
public:
	std::vector<HitInfo> hits;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( hits ) );
	}
} HITS_INFO;
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
#pragma endregion

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
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
	CREATE_STAGE_REQ,              // 방 생성 요청
	CREATE_STAGE_ACK,              // 방 생성 응답
	UPDATE_STAGE_INFO,             // 방 정보가 갱신됨
	INSERT_STAGE_INFO,             // 방 정보가 추가됨
	DELETE_STAGE_INFO,             // 방 정보가 삭제됨
	ENTRY_STAGE_REQ,               // 방 입장 요청
	ENTRY_STAGE_ACK,               // 방 입장 응답
	EXIT_STAGE_REQ,                // 방 퇴장 요청
	EXIT_STAGE_ACK,                // 방 퇴장 응답
	CHANGE_HOST_ACK,               // 호스트 변경 응답

	// Actor
	SPAWN_ACTOR_REQ,               // Actor 스폰 요청
	SPAWN_ACTOR_ACK,               // Actor 스폰 응답
	RESPAWN_ACTOR_REQ,             // Actor 리스폰 요청
	RESPAWN_ACTOR_ACK,             // Actor 리스폰 응답
	SPAWN_PLAYER_REQ,              // Player 스폰 요청
	SPAWN_PLAYER_ACK,              // Player 스폰 응답
	SPAWN_BULLET_REQ,              // Bullet 스폰 요청
	SPAWN_BULLET_ACK,              // Bullet 스폰 응답
	REMOVE_ACTORS_REQ,             // Actor들 제거 요청
	REMOVE_ACTORS_ACK,             // Actor들 제거 응답

	SYNC_MOVEMENT_REQ,             // Actor 이동 동기화 요청
	SYNC_MOVEMENT_ACK,             // Actor 이동 동기화 응답
	SYNC_RELOAD_REQ,               // Player 재장전 동기화 요청
	SYNC_RELOAD_ACK,               // Player 재장전 동기화 응답
	SYNC_LOOK_ANGLE_REQ,           // Player 시선 동기화 요청
	SYNC_LOOK_ANGLE_ACK,           // Player 시선 동기화 응답
	SYNC_DODGE_ACTION_REQ,         // Player 회피 동기화 요청
	SYNC_DODGE_ACTION_ACK,         // Player 회피 동기화 응답
	SYNC_SWAP_WEAPON_REQ,          // Player 무기 교체 동기화 요청
	SYNC_SWAP_WEAPON_ACK,          // Player 무기 교체 동기화 응답
	SYNC_INTERACTION_REQ,          // InteractableActor 상호작용 요청
	SYNC_INTERACTION_ACK,          // InteractableActor 상호작용 응답
	SYNC_EATABLE_TARGET_REQ,       // EatableActor Target 설정 요청
	SYNC_EATABLE_TARGET_ACK,       // EatableActor Target 설정 응답
	SYNC_EATABLE_EVENT_REQ,        // EatableActor Event 실행 요청
	SYNC_EATABLE_EVENT_ACK,        // EatableActor Event 실행 응답
	HIT_ACTORS_REQ,                // 피격된 Actor들 동기화 요청
	HIT_ACTORS_ACK,                // 피격된 Actor들 동기화 응답

	INIT_SCENE_ACTORS_REQ,         // 씬에 배치된 Actor들 초기화 요청
	INIT_SCENE_ACTORS_ACK,         // 씬에 배치된 Actor들 초기화 응답
	INGAME_LOAD_DATA_REQ,          // InGame 입장시 데이터 요청
	GAME_OVER_ACK,                 // 게임 종료 응답
	UPDATE_RESULT_INFO_REQ,        // 게임 결과 갱신 요청
	UPDATE_RESULT_INFO_ACK,        // 게임 결과 갱신 응답
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
	LOGIN_INFO loginInfo;
	USER_INFO userInfo;

	template <class Archive>
	void serialize( Archive& ar )
	{
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
	SerialType stageSerial;
	SerialType hostSerial;
	std::string title;
	int targetKill;
	int currentKill;
	Personnel personnel;

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( stageSerial ) );
		ar( CEREAL_NVP( hostSerial ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( targetKill ) );
		ar( CEREAL_NVP( currentKill ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;
#pragma endregion

#pragma region Actor
enum class ActorType : u_short
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

	// 서버에서만 사용
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
	bool isExplode;
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
		ar( CEREAL_NVP( isExplode ) );
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
typedef struct InteractionInfo
{
public:
	SerialType serial;
	SerialType target;
	float angle;
	Vector2 pos;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( target ) );
		ar( CEREAL_NVP( angle ) );
		ar( CEREAL_NVP( pos ) );
	}
} INTERACTION_INFO;
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
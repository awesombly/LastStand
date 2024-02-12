#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // �ֱ����� ����� ���� ��Ŷ
	PACKET_CHAT_MSG,               // ä�� �޼���
							       
	CONFIRM_LOGIN_REQ = 1000,      // �α��� ��û
	CONFIRM_LOGIN_ACK,             // �α��� ����
	CONFIRM_ACCOUNT_REQ,           // ���� ���� ��û
	CONFIRM_ACCOUNT_ACK,           // ���� ���� ����
	DUPLICATE_EMAIL_REQ,           // �̸��� �ߺ�Ȯ�� ��û
	DUPLICATE_EMAIL_ACK,           // �̸��� �ߺ�Ȯ�� ����
							       
	LOBBY_INFO_REQ = 2000,         // �κ� ���� ��û
	LOBBY_INFO_ACK,                // �κ� ���� ����

	CREATE_ROOM_REQ = 3000,        // �� ���� ��û
	CREATE_ROOM_ACK,               // �� ���� ����
	UPDATE_ROOM_INFO,              // �� ������ ���ŵ�
	INSERT_ROOM_INFO,              // �� ������ �߰���
	DELETE_ROOM_INFO,              // �� ������ ������
	ENTRY_ROOM_REQ,                // �� ���� ��û
	ENTRY_ROOM_ACK,                // �� ���� ����
	EXIT_ROOM_REQ,                 // �� ���� ��û
	EXIT_ROOM_ACK,                 // �� ���� ����

	SPAWN_ENEMY_REQ = 5000,       // �� ���� ��û
	SPAWN_ENEMY_ACK,              // �� ���� ����
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
	
	Personnel() : current( 0 ), maximum( 0 ) { }
	Personnel( int _cur, int _max ) : current( _cur ), maximum( _max ) { }

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

typedef struct LobbyInfo
{
public:
	std::list<ROOM_INFO> infos;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( infos ) );
	}
} LOBBY_INFO;

typedef struct SpawnEnemy
{
public:
	int prefab;
	u_int serial;
	float x;
	float y;

	template <class Archive>
	void serialize(Archive& ar)
	{
		ar(CEREAL_NVP(prefab));
		ar(CEREAL_NVP(serial));
		ar(CEREAL_NVP(x));
		ar(CEREAL_NVP(y));
	}
} SPAWN_ENEMY;
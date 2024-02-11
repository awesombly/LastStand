#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // �ֱ����� ����� ���� ��������
	PACKET_CHAT_MSG,               // �Ϲ����� ä�� �޼���
							       
	CONFIRM_LOGIN_REQ = 1000,      // �α��� Ȯ�� ��û
	CONFIRM_LOGIN_RES,             // �α��� Ȯ�� ����
	DUPLICATE_EMAIL_REQ,           // �̸��� �ߺ�Ȯ�� ��û
	DUPLICATE_EMAIL_RES,           // �̸��� �ߺ�Ȯ�� ����
	CONFIRM_SIGNUP_REQ,            // �̸��� ���� ��û
	CONFIRM_SIGNUP_RES,            // �̸��� ���� ����
							       
	CREATE_ROOM_REQ = 2000,        // �� ���� ��û
	CREATE_ROOM_RES,               // �� ���� ����
	TAKE_ROOM_LIST,                // �� ��� ����
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
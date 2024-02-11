#pragma once
#include "Global/Header.h"

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT, // �ֱ����� ����� ���� ��������
	PACKET_CHAT_MSG,  // �Ϲ����� ä�� �޼���

	CONFIRM_LOGIN_REQ = 1000, // �α��� Ȯ�� ��û
	CONFIRM_LOGIN_RES,        // �α��� Ȯ�� ����
	DUPLICATE_EMAIL_REQ,      // �̸��� �ߺ�Ȯ�� ��û
	DUPLICATE_EMAIL_RES,      // �̸��� �ߺ�Ȯ�� ����
	CONFIRM_SIGNUP_REQ,       // �̸��� ���� ��û
	CONFIRM_SIGNUP_RES,       // �̸��� ���� ����

	CREATE_ROOM_REQ = 2000, // �� ���� ��û
	CREATE_ROOM_RES,        // �� ���� ����
	TAKE_ROOM_LIST,         // �� ��� ����
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
	// Ŭ�� ������ Ȯ���ϱ� ���� ��������
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


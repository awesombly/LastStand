using System.Collections.Generic;
using UnityEngine;

// null�� ���Ե� �����͸� JSON���� ����� ������ �����ϴ�.
// string ���� Ŭ������ �� �ʱ�ȭ ���ּ���.

// �������� ��ŶŸ���� ���ڿ��� ����ϱ�����
// Ÿ���� 0���� ������� �����ǵ��� �մϴ�.
public enum PacketType : ushort
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
    CREATE_STAGE_REQ,              // �� ���� ��û
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
    SPAWN_PLAYER_ACK,              // Player ���� ��û
    SPAWN_PLAYER_REQ,              // Player ���� ����
    SYNK_MOVEMENT_ACK,             // Actor �̵� ����ȭ ��û
    SYNK_MOVEMENT_REQ,             // Actor �̵� ����ȭ ����
};

public struct VECTOR3
{
    public float x, y, z;

    public VECTOR3( Vector3 _vector3 )
    {
        x = _vector3.x;
        y = _vector3.y;
        z = _vector3.z;
    }

    public Vector3 To()
    {
        return new Vector3( x, y, z );
    }
}

public struct QUATERNION
{
    public float x, y, z, w;

    public QUATERNION( Quaternion _quaternion )
    {
        x = _quaternion.x;
        y = _quaternion.y;
        z = _quaternion.z;
        w = _quaternion.w;
    }

    public Quaternion To()
    {
        return new Quaternion( x, y, z, w );
    }
}

public interface IProtocol { }
// Both 
public struct EMPTY : IProtocol { }
public struct MESSAGE : IProtocol { public string message; }
public struct CONFIRM : IProtocol { public bool isCompleted; }

public struct Personnel { public int current, maximum; }
public struct STAGE_INFO : IProtocol
{
    public ushort serial;
    public string title;
    public Personnel personnel;
}

public struct LOGIN_INFO : IProtocol
{
    public string nickname;
    public string email;
    public string password;
}

public struct ACTOR_INFO : IProtocol
{
    public int prefab;
    public bool isLocal;
    public uint serial;
    public VECTOR3 position;
    public QUATERNION rotation;
    public VECTOR3 velocity;
}

public struct CHAT_MESSAGE : IProtocol
{
    public string nickname;
    public string message;
}
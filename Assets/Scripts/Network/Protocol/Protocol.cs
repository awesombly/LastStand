using System.Collections.Generic;


// null�� ���Ե� �����͸� JSON���� ����� ������ �����ϴ�.
// string ���� Ŭ������ �� �ʱ�ȭ ���ּ���.
public enum PacketType : ushort
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

public interface IProtocol { }
// Both 
public struct EMPTY : IProtocol { }
public struct MESSAGE : IProtocol { public string message; }
public struct CONFIRM : IProtocol { public bool isCompleted; }

public struct Personnel { public int current, maximum; }
public struct ROOM_INFO : IProtocol
{
    public ushort uid;
    public string title;
    public Personnel personnel;
}

public struct LOGIN_INFO : IProtocol
{
    public string nickname;
    public string email;
    public string password;
}

public struct LOBBY_INFO : IProtocol
{
    public List<ROOM_INFO> infos;
}

public struct SPAWN_ENEMY : IProtocol
{
    public int prefab;
    public int serial;
    public float x;
    public float y;
}

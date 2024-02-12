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

    CREATE_STAGE_REQ = 3000,        // �� ���� ��û
    CREATE_STAGE_ACK,               // �� ���� ����
    UPDATE_STAGE_INFO,              // �� ������ ���ŵ�
    INSERT_STAGE_INFO,              // �� ������ �߰���
    DELETE_STAGE_INFO,              // �� ������ ������
    ENTRY_STAGE_REQ,                // �� ���� ��û
    ENTRY_STAGE_ACK,                // �� ���� ����
    EXIT_STAGE_REQ,                 // �� ���� ��û
    EXIT_STAGE_ACK,                 // �� ���� ����

    SPAWN_ENEMY_REQ = 5000,       // �� ���� ��û
    SPAWN_ENEMY_ACK,              // �� ���� ����
};

public interface IProtocol { }
// Both 
public struct EMPTY : IProtocol { }
public struct MESSAGE : IProtocol { public string message; }
public struct CONFIRM : IProtocol { public bool isCompleted; }

public struct Personnel { public int current, maximum; }
public struct STAGE_INFO : IProtocol
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
    public List<STAGE_INFO> infos;
}

public struct SPAWN_ENEMY : IProtocol
{
    public int prefab;
    public int serial;
    public float x;
    public float y;
}

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

    CREATE_ROOM_REQ,               // �� ���� ��û
    CREATE_ROOM_ACK,               // �� ���� ����
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
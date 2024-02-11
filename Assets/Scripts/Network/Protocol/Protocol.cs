using System.Collections.Generic;
using UnityEngine.AI;


// null�� ���Ե� �����͸� JSON���� ����� ������ �����ϴ�.
// string ���� Ŭ������ �� �ʱ�ȭ ���ּ���.
public enum PacketType : ushort
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

//public struct ResTakeRoom : IProtocol
//{
//    public List<RoomData> rooms;
//}
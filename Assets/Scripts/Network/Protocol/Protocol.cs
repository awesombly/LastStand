using System.Collections.Generic;
using UnityEngine.AI;


// null이 포함된 데이터를 JSON으로 만들면 서버가 뻗습니다.
// string 같은 클래스는 꼭 초기화 해주세요.
public enum PacketType : ushort
{
    NONE = 0,
    PACKET_HEARTBEAT, // 주기적인 통신을 위한 프로토콜
    PACKET_CHAT_MSG,  // 일반적인 채팅 메세지

    CONFIRM_LOGIN_REQ = 1000, // 로그인 확인 요청
    CONFIRM_LOGIN_RES,        // 로그인 확인 응답
    DUPLICATE_EMAIL_REQ,      // 이메일 중복확인 요청
    DUPLICATE_EMAIL_RES,      // 이메일 중복확인 응답
    CONFIRM_SIGNUP_REQ,       // 이메일 생성 요청
    CONFIRM_SIGNUP_RES,       // 이메일 생성 응답

    CREATE_ROOM_REQ = 2000, // 방 생성 요청
    CREATE_ROOM_RES,        // 방 생성 응답
    TAKE_ROOM_LIST,         // 방 목록 전달
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
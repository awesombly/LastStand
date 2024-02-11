using System.Collections.Generic;

public static class Protocol
{
    // 서버/클라 결과 동일해야함. (Sdbm Hash)
    //public static ushort GetPacketType( string _name )
    //{
    //    uint hash = 0;
    //    foreach ( char elem in _name )
    //    {
    //        hash = elem + ( hash << 6 ) + ( hash << 16 ) - hash;
    //    }

    //    return ( ushort )hash;
    //}
}

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

public interface IProtocol
{
    //public string name => ToString();
    //public ushort type => Protocol.GetPacketType( name );
}

// Both 
public struct Heartbeat : IProtocol 
{
    // 서버 연결을 확인하기 위한 프로토콜
}

public struct ChatMessage : IProtocol
{
    public string message;
}

// 방 생성
public struct ReqMakeRoom : IProtocol
{
    public string title;
    public int maxPersonnel;
}

public struct ResMakeRoom : IProtocol
{
    public ushort uid;
    public bool isCompleted;
}

public struct ResTakeRoom : IProtocol
{
    public List<RoomData> rooms;
}

// 로그인
public struct ReqLogin : IProtocol
{
    public string email;
    public string password;
}

public struct ReqSignUp : IProtocol
{
    public string nickname;
    public string email;
    public string password;
}

public struct ReqSignUpMail : IProtocol
{
    public string email;
    public string password;
}

public struct ResLogin : IProtocol
{
    public string nickname;
}

public struct ResSignUp : IProtocol
{
    public bool isCompleted;
}

public struct ResSignUpMail : IProtocol
{
    public bool isPossible;
}
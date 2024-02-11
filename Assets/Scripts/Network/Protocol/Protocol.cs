
public static class Protocol
{
    // ����/Ŭ�� ��� �����ؾ���. (Sdbm Hash)
    public static ushort GetPacketType( string _name )
    {
        uint hash = 0;
        foreach ( char elem in _name )
        {
            hash = elem + ( hash << 6 ) + ( hash << 16 ) - hash;
        }

        return ( ushort )hash;
    }
}

public interface IProtocol
{
    public string name => ToString();
    public ushort type => Protocol.GetPacketType( name );
}

// Both 
public struct Heartbeat : IProtocol 
{
    // ���� ������ Ȯ���ϱ� ���� ��������
}

public struct ChatMessage : IProtocol
{
    public string message;
}

// �� ����
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

// �α���
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
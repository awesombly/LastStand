
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

// Request ������ ������ ��û
public struct ReqLogin : IProtocol
{
    public string email;
    public string password;
}

// Response ��û�� ���� �亯
public struct ConnectMessage : IProtocol
{
    public string message;
}

public struct ResLogin : IProtocol
{
    public string nickname;
    public string email;
    public string password;
}

// Both 
public struct ChatMessage : IProtocol
{
    public string message;
}
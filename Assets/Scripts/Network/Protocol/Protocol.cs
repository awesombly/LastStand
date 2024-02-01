using System.Xml.Linq;
using UnityEngine;

public static class Protocol
{
    // 서버/클라 결과 동일해야함. (Sdbm Hash)
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

public struct SampleProtocol : IProtocol
{
    public int money;
    public float speed;
    public string name;
}

// From Server
public struct ConnentMessage : IProtocol
{
    public string message;
}

// Both
public struct ChatMessage : IProtocol
{
    public string message;
}
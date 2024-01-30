using System.Xml.Linq;
using UnityEngine;

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
    public string Name => ToString();
    public ushort Type => Protocol.GetPacketType( Name );
}

public struct ChatMessage : IProtocol
{
    public string message;
}
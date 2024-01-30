using System.Collections;
using System.Collections.Generic;
using UnityEditor.Sprites;
using UnityEngine;

//public interface IProtocol
//{
//    ushort GetPacketType();
//}

//public struct ChatMessage : IProtocol
//{
//    public string message;

//    public static ushort PacketType = Packet.GetPacketType( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name );

//    public ushort GetPacketType()
//    {
//        return PacketType;
//    }
//}

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
}

public struct ChatMessage : IProtocol
{
    public static ushort type = Protocol.GetPacketType( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name );
}
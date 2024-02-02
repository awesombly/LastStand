using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public ushort type;
    public ushort size;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = Global.MaxDataSize )]
    public byte[] data;

    public Packet( ushort _type, ushort _size, byte[] _data )
    {
        type = _type;
        size = _size;
        data = _data;
    }

    public Packet( IProtocol _protocol )
    {
        type = _protocol.type;
        data = System.Text.Encoding.UTF8.GetBytes( JsonUtility.ToJson( _protocol ) );
        size = ( ushort )( data.Length + Global.HeaderSize );
    }
}
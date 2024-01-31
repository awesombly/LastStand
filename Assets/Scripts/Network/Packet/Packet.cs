using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public const int HeaderSize  = 4;
    public const int MaxDataSize = 2048;

    public ushort type;
    public ushort size;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = MaxDataSize )]
    public byte[] data;

    public Packet( IProtocol _protocol )
    {
        type = _protocol.type;
        data = System.Text.Encoding.UTF8.GetBytes( JsonUtility.ToJson( _protocol ) );
        size = ( ushort )( data.Length + HeaderSize );
    }
}
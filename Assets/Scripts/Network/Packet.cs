using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public class Packet
{
    public const int HeaderSize  = 4;
    public const int MaxDataSize = 2048;

    public ushort length;
    public ushort type;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = MaxDataSize )]
    public byte[] data;

    public Packet( byte[] _data )
    {
        type   = 0;
        data   = _data;
        length = ( ushort )( data.Length + HeaderSize );
    }

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

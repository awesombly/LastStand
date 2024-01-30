using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public const int HeaderSize  = 4;
    public const int MaxDataSize = 2048;

    public ushort type;
    public ushort size;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = MaxDataSize )]
    public byte[] data;

    public Packet( byte[] _data )
    {
        type = 0;
        data = _data;
        size = ( ushort )( data.Length + HeaderSize );
    }
}
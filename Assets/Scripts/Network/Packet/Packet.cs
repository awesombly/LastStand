using Newtonsoft.Json;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public PacketType type;
    public ushort size;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = Global.MaxDataSize )]
    public byte[] data;

    public Packet( PacketType _type, ushort _size, byte[] _data )
    {
        type = _type;
        size = _size;
        data = _data;
    }

    public Packet( PacketType _type, IProtocol _protocol )
    {
        type = _type;
        data = System.Text.Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( _protocol ) );
        size = data.Length > 2 ? ( ushort )( data.Length + Global.HeaderSize ) // ���� ��Ŷ
                               : ( ushort )Global.HeaderSize;                  // �� ��Ŷ�� JSON ��ȯ�� {} 2���ڸ� ��
    }
}
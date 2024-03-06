using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public Result result;
    public PacketType type;
    public ushort size;
    public byte[] data;

    public Packet( Result _result, PacketType _type, ushort _size, byte[] _data )
    {
        result = _result;
        type   = _type;
        size   = _size;
        data   = _data;
    }

    public Packet( PacketType _type )
    {
        result = Result.OK;
        type   = _type;
        size   = Global.HeaderSize;

        data = new byte[size];
        BitConverter.TryWriteBytes( new Span<byte>( data, 0, sizeof( ushort ) ), ( ushort )result );
        BitConverter.TryWriteBytes( new Span<byte>( data, 2, sizeof( ushort ) ), ( ushort )type );
        BitConverter.TryWriteBytes( new Span<byte>( data, 4, sizeof( ushort ) ), ( ushort )size );
    }

    public Packet( PacketType _type, IProtocol _protocol )
    {
        result = Result.OK;
        type = _type;
        byte[] json = System.Text.Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( _protocol ) );
        size = json.Length > 2 ? ( ushort )( json.Length + Global.HeaderSize ) // 정상 패킷
                               : ( ushort )Global.HeaderSize;                  // 빈 패킷은 JSON 변환시 {} 2문자만 들어감

        data = new byte[size];
        BitConverter.TryWriteBytes( new Span<byte>( data, 0, sizeof( ushort ) ), ( ushort )result );
        BitConverter.TryWriteBytes( new Span<byte>( data, 2, sizeof( ushort ) ), ( ushort )type );
        BitConverter.TryWriteBytes( new Span<byte>( data, 4, sizeof( ushort ) ), ( ushort )size );

        if ( json.Length > 2 )
            Buffer.BlockCopy( json, 0, data, Global.HeaderSize, json.Length );
    }
}
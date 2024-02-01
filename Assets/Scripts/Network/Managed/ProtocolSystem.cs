using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class ProtocolSystem : Singleton<ProtocolSystem>
{
    private Dictionary<ushort/* Packet Type */, Action<Packet>> protocols = new Dictionary<ushort, Action<Packet>>();

    protected override void Awake()
    {
        base.Awake();
        Regist( new SampleProtocol(), SampleProcess );
    }

    private void SampleProcess( Packet _packet )
    {
        var sample = Global.Deserialize<SampleProtocol>( _packet.data, 0 );
        Debug.Log( $"name : {sample.name}  speed : {sample.speed}  money : {sample.money}" );
    }


    public void Regist( IProtocol _protocol, Action<Packet> _func )
    {
        if ( protocols.ContainsKey( _protocol.type ) )
        {
            Debug.LogWarning( $"The {_protocol.type} protocol is dulicated." );
            return;
        }

        protocols.Add( _protocol.type, _func );
    }

    public void Process( in Packet _packet )
    {
        if ( !protocols.ContainsKey( _packet.type ) )
        {
            Debug.LogWarning( $"The {_packet.type} protocol is not registered." );
            return;
        }

        protocols[_packet.type]?.Invoke( _packet );
    }
}
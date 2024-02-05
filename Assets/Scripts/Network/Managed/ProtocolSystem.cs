using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolSystem : Singleton<ProtocolSystem>
{
    private Dictionary<ushort/* Packet Type */, Action<Packet>> protocols = new Dictionary<ushort, Action<Packet>>();
    
    protected override void Awake()
    {
        base.Awake();
        Regist( new SampleProtocol(), SampleProcess );
        Regist( new ConnectMessage(), OnConnected );
    }

    private void SampleProcess( Packet _packet )
    {
        SampleProtocol data = Global.FromJson<SampleProtocol>( _packet );
        Debug.Log( $"name : {data.name}  speed : {data.speed}  money : {data.money}" );

        SampleProtocol sample;
        sample.name = "sample";
        sample.money = 2000;
        sample.speed = 52.6f;
        Network.Inst.Send( new Packet( sample ) );
    }

    private void OnConnected( Packet _packet )
    {
        ConnectMessage message = Global.FromJson<ConnectMessage>( _packet );
        Debug.Log( $"{message.message}" );

        Network.Inst.Send( new Packet( message ) );
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
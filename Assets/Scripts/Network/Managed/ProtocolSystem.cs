using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProtocolSystem : Singleton<ProtocolSystem>
{
    private Dictionary<PacketType, Action<Packet>> protocols = new Dictionary<PacketType, Action<Packet>>();
    
    public void Regist( PacketType _type, Action<Packet> _func )
    {
        if ( protocols.ContainsKey( _type ) )
        {
            protocols[_type] = _func;
            return;
        }

        protocols.Add( _type, _func );
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
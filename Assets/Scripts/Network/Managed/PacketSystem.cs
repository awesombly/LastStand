using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor.Sprites;
using UnityEngine;

using static PacketType;
public class PacketSystem : Singleton<PacketSystem>
{
    private Queue<Packet> packets = new Queue<Packet>();
    
    protected override void Awake()
    {
        base.Awake();

        StartCoroutine( Process() );
    }

    private IEnumerator Process()
    {
        yield return new WaitUntil( () => { return Network.Inst.IsConnected; } );

        WaitUntil waitReceivePackets = new WaitUntil( () => { return packets.Count > 0; } );
        while ( Network.Inst.IsConnected )
        {
            yield return waitReceivePackets;

            ProtocolSystem.Inst.Process( packets.Dequeue() );
        }
    }

    public void Push( in Packet _packet )
    {
        if ( _packet.type != PACKET_HEARTBEAT )
             Debug.Log( $"Receive ( {_packet.type}, {_packet.size} bytes ) {System.Text.Encoding.UTF8.GetString( _packet.data )}" );

        packets.Enqueue( _packet );
    }
}
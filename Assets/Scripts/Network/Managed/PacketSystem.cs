using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            var packet = packets.Dequeue();
            Debug.Log( $"Receive ( {packet.type}, {packet.size} bytes ) {System.Text.Encoding.UTF8.GetString( packet.data )}" );

            ProtocolSystem.Inst.Process( packet );
        }
    }

    public void Push( in Packet _packet )
    {
        if ( _packet.size > 0 )
             packets.Enqueue( _packet );
    }
}
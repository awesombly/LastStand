using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PacketSystem : Singleton<PacketSystem>
{
    private Queue<Packet> packets = new Queue<Packet>();
    
    protected override void Awake()
    {
        base.Awake();

        StartCoroutine( Process() );
    }

    private IEnumerator Process()
    {
        WaitUntil waitConnectNetwork = new WaitUntil( () => { return Network.Inst.IsConnected; } );
        WaitUntil waitReceivePackets = new WaitUntil( () => { return packets.Count > 0; } );
        while ( true )
        {
            if ( !Network.Inst.IsConnected )
                 yield return waitConnectNetwork;

            yield return waitReceivePackets;
            while ( packets.Count > 0 )
            {
                ProtocolSystem.Inst.Process( packets.Dequeue() );
            }
        }
    }

    public void Push( in Packet _packet )
    {
        packets.Enqueue( _packet );
    }
}
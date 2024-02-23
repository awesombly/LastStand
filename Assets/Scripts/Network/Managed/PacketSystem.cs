using System.Collections;
using System.Collections.Generic;
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

            while( packets.Count > 0 )
                   ProtocolSystem.Inst.Process( packets.Dequeue() );
        }
    }

    public void Push( in Packet _packet )
    {
        packets.Enqueue( _packet );
    }
}
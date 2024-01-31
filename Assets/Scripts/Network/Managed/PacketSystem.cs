using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PacketSystem : Singleton<PacketSystem>
{
    private Queue<Packet> packets = new Queue<Packet>();
    private object cs = new object();
    private Thread thread;
    
    private void Start()
    {
        StartCoroutine( WaitConnectServer() );
    }

    private void OnDestroy()
    {
        thread?.Abort();
    }

    private IEnumerator WaitConnectServer()
    {
        yield return new WaitUntil( () => Network.Inst.IsConnected );

        thread = new Thread( new ThreadStart( Process ) );
        thread.Start();
    }

    public void Push( in Packet _packet )
    {
        lock ( cs ) { packets.Enqueue( _packet ); }
    }

    private void Process()
    {
        while( packets.Count > 0 )
        {
            var packet = packets.Dequeue();
            Debug.Log( $"Receive ( {packet.type}, {packet.size} bytes ) {packet.data}" );

            lock ( cs ) { ProtocolSystem.Inst.Process( packet ); }
        }
    }
}
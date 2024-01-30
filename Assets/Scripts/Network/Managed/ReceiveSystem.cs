using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ReceiveSystem : Singleton<ReceiveSystem>
{
    private const ushort MaxReceiveSize = 10000;

    private Thread thread;
    private byte[] buffer = new byte[MaxReceiveSize];

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

        thread = new Thread( new ThreadStart( Receive ) );
        thread.Start();
    }

    private void Receive()
    {
        while ( true )
        {
            // 우선 그냥 나가도록 구현.
            if ( !Network.Inst.IsConnected )
                 break;

            //if ( Network.Inst.Receive( ref buffer )  )
                 return;
            
            //Packet packet = Global.Deserialize<Packet>( buffer, offset );


            //// TODO : 패킷이 중간에 짤린 경우에 대한 처리. (buffer length 이상 들어올시)
            //int offset = 0;
            //while ( true )
            //{
            //    Packet packet = Global.Deserialize<Packet>( buffer, offset );
            //    if ( ReferenceEquals( packet, null ) || packet.size == 0 )
            //    {
            //        break;
            //    }

            //    if ( packet.size > buffer.Length )
            //    {
            //        Debug.LogError( "buffer overflow. packet = " + packet.size + ", buffer = " + buffer.Length );
            //        break;
            //    }

            //    //string data = System.Text.Encoding.UTF8.GetString( packet.data, 0, packet.length - Packet.HeaderSize );
            //    PacketSystem.Inst.Push( packet );

            //    System.Array.Clear( buffer, offset, packet.size );
            //    offset += packet.size;
            //}
        }
    }
}

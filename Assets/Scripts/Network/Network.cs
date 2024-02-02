using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.Sprites;
using UnityEngine;

public class Network : Singleton<Network>
{
    private const int    Port           = 10000;
    private const string Ip             = "127.0.0.1"; // 콜백 주소
    private const ushort MaxReceiveSize = 10000;

    private Socket socket;
    SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs recvArgs    = new SocketAsyncEventArgs();
    SocketAsyncEventArgs sendArgs    = new SocketAsyncEventArgs();

    // Receive
    private byte[] buffer = new byte[MaxReceiveSize];
    private int startPos, writePos, readPos;
    private Packet packet;
    public bool IsConnected => socket != null && socket.Connected;

    private void Start()
    {
        Connect();

        //StartCoroutine( Example() );
    }

    private IEnumerator Example()
    {
        yield return new WaitForSeconds( 1f );
        ConnectMessage message;
        message.message = "Connect Server";
        Send( new Packet( message ) );

        yield return new WaitForSeconds( 1f );
        Send( new Packet( message ) );
    }

    private void OnDestroy()
    {
        if ( !ReferenceEquals( socket, null ) )
             socket.Close();
    }

    private void Connect()
    {
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( Ip ), Port );

        connectArgs.RemoteEndPoint = point;
        connectArgs.Completed += OnConnecteCompleted;
        socket.ConnectAsync( connectArgs );
    }

    private void OnConnecteCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.SocketError == SocketError.Success )
        {
            Debug.Log( $"Server Connect" );

            // Send
            sendArgs.Completed += OnSendCompleted;

            // Receive
            recvArgs.SetBuffer( buffer, 0, MaxReceiveSize );
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnReceiveCompleted );

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
    }

    private void OnReceiveCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.BytesTransferred > 0 && _args.SocketError == SocketError.Success )
        {
            int size = _args.BytesTransferred;
            if ( writePos + size > MaxReceiveSize )
            {
                byte[] remain = new byte[MaxReceiveSize];
                Array.Copy( buffer, remain, readPos );

                Array.Clear( buffer, 0, MaxReceiveSize );
                Array.Copy( remain, buffer, readPos );

                packet = Global.Deserialize<Packet>( buffer, 0 );
                startPos = 0;
                writePos = readPos;
            }

            Array.Copy( _args.Buffer, 0, buffer, writePos, size );
            packet    = Global.Deserialize<Packet>( buffer, startPos );
            writePos += size;
            readPos  += size;

            if ( readPos >= packet.size )
            {
                do
                {
                    byte[] data = new byte[packet.size - Global.HeaderSize];
                    Array.Copy( packet.data, data, packet.size - Global.HeaderSize );
                    PacketSystem.Inst.Push( new Packet( packet.type, packet.size, data ) );
                    
                    readPos  -= packet.size;
                    startPos += packet.size;

                    packet = Global.Deserialize<Packet>( buffer, startPos );
                    if ( packet.size <= 0 ) break;

                } while ( readPos >= packet.size );
            }

            _args.BufferList = null;

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
    }

    private void OnSendCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        Debug.Log( $"Send" );
        if ( _args.BytesTransferred > 0 && _args.SocketError == SocketError.Success )
             _args.BufferList = null;
    }

    public void Send( Packet _packet )
    {
        byte[] data = Global.Serialize( _packet );
        sendArgs.SetBuffer( data, 0, data.Length );
        
        socket.SendAsync( sendArgs );
    }
}

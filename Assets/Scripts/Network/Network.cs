using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Network : Singleton<Network>
{
    private int    port = 10000;
    private string ip   = "127.0.0.1"; // 콜백 주소

    //private Thread thread;
    public Socket socket { get; private set; }

    public bool IsConnected => socket != null && socket.Connected;

    private void Start()
    {
        Connect();
    }

    private void OnDestroy()
    {
        if ( !ReferenceEquals( socket, null ) )
             socket.Close();
    }

    private void Connect()
    {
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( ip ), port );

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = point;
        args.Completed += OnConnecteCompleted;
        socket.ConnectAsync( args );
    }

    private void OnConnecteCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.SocketError == SocketError.Success )
        {
            Debug.Log( $"Server Connect" );

            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += OnSendCompleted;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.SetBuffer( new byte[1024], 0, 1024 );
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnSendCompleted );

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
    }

    private void OnSendCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        Debug.Log( $"Receive" );
        if ( _args.BytesTransferred <= 0 || _args.SocketError != SocketError.Success )
            return;

        string data = Encoding.UTF8.GetString( _args.Buffer, _args.Offset, _args.BytesTransferred );
        Debug.Log( data );
    }

    private void OnReceiveCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        Debug.Log( $"Receive" );
        if ( _args.BytesTransferred <= 0 || _args.SocketError != SocketError.Success )
             return;

        string data = Encoding.UTF8.GetString( _args.Buffer, _args.Offset, _args.BytesTransferred );
        Debug.Log( data );
    }

    //public bool Receive( ref byte[] _buffer )
    //{
    //    if ( !IsConnected )
    //         return false;

    //    socket.Receive( _buffer, 0, _buffer.Length, SocketFlags.None, out SocketError error );
    //    socket.ReceiveAsync( );
    //    Debug.Log( _buffer.Length );
    //    if ( error != SocketError.Success )
    //    {
    //        Debug.LogError( error.ToString() );
    //        return false;
    //    }

    //    return true;
    //}

    //private void Receive()
    //{
    //    while ( true )
    //    {
    //        SocketError error;
    //        socket.Receive( buffer, 0, buffer.Length, SocketFlags.None, out error );
    //        if ( error != SocketError.Success )
    //        {
    //            Debug.LogError( "Socket receive failed. error = " + error.ToString() );
    //            return;
    //        }

    //        // TODO : 패킷이 중간에 짤린 경우에 대한 처리. (buffer length 이상 들어올시)
    //        int offset = 0;
    //        while ( true )
    //        {
    //            Packet packet = Global.Deserialize<Packet>( buffer, offset );
    //            if ( ReferenceEquals( packet, null ) || packet.size == 0 )
    //            {
    //                break;
    //            }

    //            if ( packet.size > buffer.Length )
    //            {
    //                Debug.LogError( "buffer overflow. packet = " + packet.size + ", buffer = " + buffer.Length );
    //                break;
    //            }

    //            //string data = System.Text.Encoding.UTF8.GetString( packet.data, 0, packet.length - Packet.HeaderSize );
    //            packets.Enqueue( packet );

    //            System.Array.Clear( buffer, offset, packet.size );
    //            offset += packet.size;
    //        }
    //    }
    //}

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            ChatMessage message;
            message.message = "ABCDEFG";

            Send( message );
        }
    }

    private void Send( IProtocol _protocol )
    {
        if ( !IsConnected )
             return;

        Packet packet = new Packet( _protocol );
        byte[] data   = Global.Serialize( packet );
        if ( ReferenceEquals( data, null ) )
             return;

        socket.Send( data );
    }
}

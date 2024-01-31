using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Network : Singleton<Network>
{
    private int    port = 10000;
    private string ip   = "127.0.0.1"; // 妮归 林家

    public Socket socket { get; private set; }
    SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

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

            // Send
            sendArgs.Completed += OnSendCompleted;

            // Receive
            recvArgs.SetBuffer( new byte[1024], 0, 1024 );
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnReceiveCompleted );

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
    }

    private void OnReceiveCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.BytesTransferred > 0 && _args.SocketError == SocketError.Success )
        {
            string data = Encoding.UTF8.GetString( _args.Buffer, _args.Offset, _args.BytesTransferred );
            
            // 单捞磐 贸府

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

    private void Send( IProtocol _protocol )
    {
        Packet packet = new Packet( _protocol );
        byte[] data   = Global.Serialize( packet );
        sendArgs.SetBuffer( data, 0, data.Length );
        
        socket.SendAsync( sendArgs );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            ChatMessage message;
            message.message = "ABCDEFG";

            Send( message );
        }
    }
}

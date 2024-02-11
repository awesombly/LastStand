using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

using static PacketType;
public class Network : Singleton<Network>
{
    private const int    Port           = 10000;
    private const string Ip             = "127.0.0.1"; // 콜백 주소
    private const ushort MaxReceiveSize = 10000;

    private Socket socket;
    SocketAsyncEventArgs connectArgs;
    SocketAsyncEventArgs recvArgs;
    SocketAsyncEventArgs sendArgs;

    // Receive
    private byte[] buffer = new byte[MaxReceiveSize];
    private int startPos, writePos, readPos;
    private Packet packet;
    
    public bool IsConnected => isConnected;
    private bool isConnected;
    private bool shouldReconnect;
    private readonly float ReconnectDelay = 3f;

    private double LastResponseTime => ( DateTime.Now.TimeOfDay.TotalSeconds - lastResponseSystemTime );
    private double lastResponseSystemTime;
    private static readonly float ResponseTimeout = 60f;

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine( ConfirmDisconnect() );
        StartCoroutine( ReconnectProcess() );

        Connect();
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( PACKET_HEARTBEAT, ( Packet ) => { Send( new Packet( PACKET_HEARTBEAT, new Heartbeat() ) ); } );
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Release()
    {
        connectArgs?.Dispose();
        recvArgs?.Dispose();
        sendArgs?.Dispose();

        socket?.Close();
    }

    private void Connect()
    {
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.DontLinger, true );
        socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.Linger,     false );
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( Ip ), Port );

        connectArgs = new SocketAsyncEventArgs();
        connectArgs.RemoteEndPoint = point;
        connectArgs.Completed += OnConnectCompleted;

        socket.ConnectAsync( connectArgs );
    }

    private void OnConnectCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.SocketError == SocketError.Success )
        {
            Debug.Log( $"Server connection completed" );
            
            lastResponseSystemTime = DateTime.Now.TimeOfDay.TotalSeconds;
            shouldReconnect = false;
            isConnected     = true;

            // Send
            sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += OnSendCompleted;

            // Receive
            recvArgs = new SocketAsyncEventArgs();
            recvArgs.SetBuffer( buffer, 0, MaxReceiveSize );
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnReceiveCompleted );

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
        else
        {
            Debug.LogWarning( $"Server connection failed" );
            shouldReconnect = true;
        }
    }

    private void OnReceiveCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        lastResponseSystemTime = DateTime.Now.TimeOfDay.TotalSeconds;
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
        if ( _args.BytesTransferred > 0 && _args.SocketError == SocketError.Success )
             _args.BufferList = null;
    }

    public void Send( Packet _packet )
    {
        if ( _packet.type != PACKET_HEARTBEAT )
             Debug.Log( $"Send ( {_packet.type}, {_packet.size} bytes ) {System.Text.Encoding.UTF8.GetString( _packet.data )}" );

        byte[] data = Global.Serialize( _packet );
        sendArgs.SetBuffer( data, 0, data.Length );
        
        socket.SendAsync( sendArgs );
    }

    #region KeepAlive Server
    private IEnumerator ReconnectProcess()
    {
        WaitUntil waitReconnectTiming = new WaitUntil( () => shouldReconnect );
        while ( true )
        {
            yield return waitReconnectTiming;

            shouldReconnect = false;

            Debug.Log( $"Reconnect to the server" );
            if ( socket.ConnectAsync( connectArgs ) == false )
                 OnConnectCompleted( null, connectArgs );

            yield return YieldCache.WaitForSeconds( ReconnectDelay );
        }
    }

    private IEnumerator ConfirmDisconnect()
    {
        WaitUntil waitConnected = new WaitUntil( () => isConnected );
        while ( true )
        {
            yield return waitConnected;

            if ( LastResponseTime > ResponseTimeout )
            {
                isConnected     = false;
                shouldReconnect = false;
                Release();

                Debug.LogError( $"Server connection is not smooth" );
                Connect();
            }
        }
    }
    #endregion
}

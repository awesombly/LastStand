using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

using static PacketType;


public sealed class Network : Singleton<Network>
{
    public enum IpType { NONE/* 콜백 주소 */, WNS, TAE, }
    public IpType ip = IpType.NONE;

    private Socket       socket;
    private string       Ip;
    private const int    Port           = 10000;
    private const ushort MaxReceiveSize = 10000;

    // Receive
    private byte[] buffer  = new byte[MaxReceiveSize];     
    private byte[] recvBuf = new byte[Global.MaxDataSize]; 
    private int startPos, writePos, readPos;               

    // Connect
    public  bool IsConnected => isConnected;
    private bool isConnected;
    private bool shouldReconnect;
    private readonly float ReconnectDelay = 3f;

    public event Action OnConnected, OnReconnected, OnDisconnected;

    private SocketAsyncEventArgs connectArgs;
    private SocketAsyncEventArgs recvArgs;
    private SocketAsyncEventArgs sendArgs;

    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

    private double LastResponseTime => ( DateTime.Now.TimeOfDay.TotalSeconds - lastResponseSystemTime );
    private double lastResponseSystemTime;
    private static readonly float ResponseTimeout = 60f;

    private object _lock = new object();

    protected override void Awake()
    {
        base.Awake();
        
       Ip = ip == IpType.WNS ? "114.199.144.213" :
            ip == IpType.TAE ? "49.142.77.208"   :
                               "127.0.0.1";

        var protocolSystem = ProtocolSystem.Inst;
        var packetSystem   = PacketSystem.Inst;
        var audioManager   = AudioManager.Inst;

        StartCoroutine( ConfirmDisconnect() );
        StartCoroutine( ReconnectProcess() );

        Connect();
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( PACKET_HEARTBEAT, ( Packet ) => { Send( new Packet( PACKET_HEARTBEAT, new EMPTY() ) ); } );
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
            
            if ( shouldReconnect ) OnReconnected?.Invoke();
            else                   OnConnected?.Invoke();

            lastResponseSystemTime = DateTime.Now.TimeOfDay.TotalSeconds;
            shouldReconnect = false;
            isConnected     = true;

            sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += OnSendCompleted;

            recvArgs = new SocketAsyncEventArgs();
            recvArgs.SetBuffer( recvBuf, 0, Global.MaxDataSize );
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
            int recvSize = _args.BytesTransferred;
            if ( writePos + recvSize > MaxReceiveSize )
            {
                byte[] remain = new byte[readPos];
                Buffer.BlockCopy( buffer, startPos, remain, 0, readPos );
                Buffer.BlockCopy( remain, 0, buffer, 0, readPos );

                startPos = 0;
                writePos = readPos;
            }

            Buffer.BlockCopy( _args.Buffer, 0, buffer, writePos, recvSize );
            writePos += recvSize;
            readPos  += recvSize;

            ushort size = BitConverter.ToUInt16( buffer, startPos + 2 );
            if ( readPos >= size )
            {
                do
                {
                    byte[] copy = new byte[size - Global.HeaderSize];
                    Buffer.BlockCopy( buffer, startPos + Global.HeaderSize, copy, 0, size - Global.HeaderSize );
                    PacketSystem.Inst.Push( new Packet( ( PacketType )BitConverter.ToUInt16( buffer, startPos ), size, copy ) );

                    startPos += size;
                    readPos  -= size;
                    if ( readPos <= 0 || readPos < Global.HeaderSize )
                         break;

                    size = BitConverter.ToUInt16( buffer, startPos + 2 );
                } while ( readPos >= size );
            }

            _args.BufferList = null;

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
    }

    private void OnSendCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        lock ( _lock )
        {
            if ( _args.BytesTransferred > 0 && _args.SocketError == SocketError.Success )
            {
                _args.BufferList = null;
                pendingList.Clear();

                if ( sendQueue.Count > 0 )
                     PostSend();
            }
        }
    }

    public void Send( in Packet _packet )
    {
        //if ( _packet.type != PACKET_HEARTBEAT )
        //Debug.Log( $"Send ( {_packet.type}, {_packet.size} bytes ) {System.Text.Encoding.UTF8.GetString( _packet.data )}" );

        lock ( _lock )
        {
            sendQueue.Enqueue( _packet.data );
            if ( pendingList.Count == 0 )
                 PostSend();
        }
    }

    private void PostSend()
    {
        while ( sendQueue.Count > 0 )
        {
            byte[] buf = sendQueue.Dequeue();
            pendingList.Add( new ArraySegment<byte>( buf, 0, buf.Length ) );
        }

        sendArgs.BufferList = pendingList;
        if ( socket.SendAsync( sendArgs ) == false )
             OnSendCompleted( null, sendArgs );
    }

    public void Send( PacketType _type, in IProtocol _protocol )
    {
        Send( new Packet( _type, _protocol ) );
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
                OnDisconnected?.Invoke();

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

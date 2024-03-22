using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public sealed class Network : Singleton<Network>
{
    private Socket       socket;
    private const int    Port           = 10000;
    private const ushort MaxReceiveSize = 10000;

    // Receive
    private byte[] buffer  = new byte[MaxReceiveSize];     
    private byte[] recvBuf = new byte[Global.MaxDataSize]; 
    private int startPos, writePos, readPos;               

    // Connect
    public  bool IsConnected => isConnected;
    private bool isConnected;

    public event Action OnConnected, OnDisconnected;

    private SocketAsyncEventArgs connectArgs;
    private SocketAsyncEventArgs recvArgs;
    private SocketAsyncEventArgs sendArgs;

    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

    private object _lock = new object();

    protected override void Awake()
    {
        base.Awake();

        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.DontLinger, true );
        
        connectArgs = new SocketAsyncEventArgs();
        connectArgs.Completed += OnConnectCompleted;

        sendArgs = new SocketAsyncEventArgs();
        sendArgs.Completed += OnSendCompleted;

        recvArgs = new SocketAsyncEventArgs();
        recvArgs.SetBuffer( recvBuf, 0, Global.MaxDataSize );
        recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnReceiveCompleted );
    }

    public void SelectIP( string _ip )
    {
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( _ip ), Port );
        connectArgs.RemoteEndPoint = point;

        Connect();
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
        socket.ConnectAsync( connectArgs );
    }

    private void OnConnectCompleted( object _sender, SocketAsyncEventArgs _args )
    {
        if ( _args.SocketError == SocketError.Success )
        {
            Debug.Log( $"Server connection completed" );

            isConnected = true;
            OnConnected?.Invoke();

            if ( socket.ReceiveAsync( recvArgs ) == false )
                 OnReceiveCompleted( null, recvArgs );
        }
        else
        {
            Debug.LogWarning( $"Server connection failed" );
            OnDisconnected?.Invoke();
        }
    }

    private void OnReceiveCompleted( object _sender, SocketAsyncEventArgs _args )
    {
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

            ushort size = BitConverter.ToUInt16( buffer, startPos + 4 );
            if ( readPos >= size )
            {
                do
                {
                    byte[] copy = new byte[size - Global.HeaderSize];
                    Buffer.BlockCopy( buffer, startPos + Global.HeaderSize, copy, 0, size - Global.HeaderSize );
                    PacketSystem.Inst.Push( new Packet(
                        ( Result     )BitConverter.ToUInt16( buffer, startPos ),
                        ( PacketType )BitConverter.ToUInt16( buffer, startPos + 2 ), size, copy ) );

                    startPos += size;
                    readPos  -= size;
                    if ( readPos <= 0 || readPos < Global.HeaderSize )
                         break;

                    size = BitConverter.ToUInt16( buffer, startPos + 4 );
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

    public void Send( PacketType _type, in IProtocol _protocol ) => Send( new Packet( _type, _protocol ) );

    public void Send( PacketType _type ) => Send( new Packet( _type ) );
}

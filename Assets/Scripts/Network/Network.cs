using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;


public class Network : MonoBehaviour
{
    private int    port = 10000;
    private string ip   = "127.0.0.1"; // 콜백 주소

    private Socket socket;

    private byte[] buffer = new byte[ 1024 * 16 ];
    private Queue<Packet> packets = new Queue<Packet>();

    public bool IsConnected => socket.Connected;
    public bool IsRunning = true;

    private async void Start()
    {
        await Task.Run( () => Connect() );
    }

    private void OnApplicationQuit()
    {
        IsRunning = false;
    }

    private void Connect()
    {
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( ip), port );

        while ( IsRunning && !socket.Connected )
        {
            try
            {
                socket.Connect( point );
            }
            catch ( SocketException _ex )
            {
                Debug.LogError( $"Socket Connect Error( {ip} ) : {_ex.Message}" );
            }
        }

        Receive();
    }

    private void Receive()
    {
        while ( IsRunning )
        {
            SocketError error;
            socket.Receive( buffer, 0, buffer.Length, SocketFlags.None, out error );
            if ( error != SocketError.Success )
            {
                Debug.LogError( "Socket receive failed. error = " + error.ToString() );
                return;
            }

            // TODO : 패킷이 중간에 짤린 경우에 대한 처리. (buffer length 이상 들어올시)
            int offset = 0;
            while ( true )
            {
                Packet packet = Global.Deserialize<Packet>( buffer, offset );
                if ( ReferenceEquals( packet, null ) || packet.length == 0 )
                {
                    break;
                }

                if ( packet.length > buffer.Length )
                {
                    Debug.LogError( "buffer overflow. packet = " + packet.length + ", buffer = " + buffer.Length );
                    break;
                }

                //string data = System.Text.Encoding.UTF8.GetString( packet.data, 0, packet.length - Packet.HeaderSize );
                packets.Enqueue( packet );

                System.Array.Clear( buffer, offset, packet.length );
                offset += packet.length;
            }
        }
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            // Temp
            Send( new Packet( System.Text.Encoding.UTF8.GetBytes( "ABCDEFG" ) ) );
        }

        while ( packets.Count > 0 )
        {
            Packet packet = packets.Dequeue();
            Debug.Log( System.Text.Encoding.UTF8.GetString( packet.data ) );
        }
    }

    private void Send( Packet _packet )
    {
        if ( !IsConnected )
            return;

        byte[] data = Global.Serialize( _packet );
        if ( ReferenceEquals( data, null ) )
        {
            return;
        }


        socket.Send( data );
    }
}

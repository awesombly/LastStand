using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Network : MonoBehaviour
{
    private int    port = 10000;
    private string ip   = "127.0.0.1"; // 콜백 주소

    private Socket socket;

    private void Start()
    {
        StartCoroutine( Connect() );
    }

    private IEnumerator Connect()
    {
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        IPEndPoint point = new IPEndPoint( IPAddress.Parse( ip), port );

        while ( !socket.Connected )
        {
            try
            {
                socket.Connect( point );
            }
            catch ( SocketException _ex )
            {
                Debug.LogError( $"Socket Connect Error( {ip} ) : {_ex.Message}" );
            }

            yield return null;
        }
    }
}

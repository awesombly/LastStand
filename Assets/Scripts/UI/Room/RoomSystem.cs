using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using static PacketType;
using Unity.VisualScripting;

public class RoomSystem : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField title;
    public Transform contents;
    public Room prefab;
    
    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendMakeRoom = true;

    public LinkedList<Room> rooms = new LinkedList<Room>();

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( LOBBY_INFO_ACK,   AckLobbyInfo );
        ProtocolSystem.Inst.Regist( CREATE_ROOM_ACK,  AckCreateRoom );
        ProtocolSystem.Inst.Regist( UPDATE_ROOM_INFO, AckUpdateRoomInfo );
        ProtocolSystem.Inst.Regist( INSERT_ROOM_INFO, AckInsertRoomInfo );
        ProtocolSystem.Inst.Regist( DELETE_ROOM_INFO, AckDeleteRoomInfo );
    }

    private void Start()
    {
        if ( Network.Inst.IsConnected )
        {
            Network.Inst.Send( new Packet( LOBBY_INFO_REQ, new EMPTY() ) );
        }
    }

    #region Button Events
    public void ShowCreateRoomPanel( bool _active )
    {
        if ( _active )
        {
            if ( canvas.activeInHierarchy )
                 return;

            title.ActivateInputField();
            SetPersonnel( 4 );
        }
        else
        {
            title.text = string.Empty;
            title.DeactivateInputField();
        }

        canvas.SetActive( _active );
    }

    public void SetPersonnel( int _max )
    {
        maxPersonnel = _max;
        for ( int i = 0; i < personnelOutlines.Count; i++ )
        {
            personnelOutlines[i].enabled = i == _max - 1 ? true : false;
        }
    }

    public void CreateRoom()
    {
        if ( !canSendMakeRoom )
             return;

        canSendMakeRoom = false;
        ROOM_INFO protocol;
        protocol.uid       = 0;
        protocol.title     = title.text;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };
        
        Network.Inst.Send( new Packet( CREATE_ROOM_REQ, protocol ) );
    }
    #endregion

    #region Protocols
    private void AckCreateRoom( Packet _packet )
    {
        canSendMakeRoom = true;
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            // 게임 접속
        }
        else
        {
            // 실패 메세지
        }
    }

    // 로비에 처음 입장 시 서버에 등록된 방 정보 갱신
    private void AckLobbyInfo( Packet _packet )
    {
        var data = Global.FromJson<LOBBY_INFO>( _packet );
        foreach ( var info in data.infos )
        {
            // 풀링하기
            Room room = Instantiate( prefab, contents );
            room.Initialize( info );

            rooms.AddLast( room );
        }
    }

    private LinkedListNode<Room> FindRoomInfoNode( ushort _uid )
    {
        for ( var node = rooms.First; node != null; node = node.Next )
        {
            var room = node.Value;
            if ( room.info.uid == _uid )
                 return node;
        }

        return null;
    }

    #region Update Room Info
    private void AckUpdateRoomInfo( Packet _packet )
    {
        var data = Global.FromJson<ROOM_INFO>( _packet );

        var node = FindRoomInfoNode( data.uid );
        if ( node == null )
             return;

        node.Value.Initialize( data );
    }

    private void AckInsertRoomInfo( Packet _packet )
    {
        var data = Global.FromJson<ROOM_INFO>( _packet );
        Room newRoom = Instantiate( prefab, contents );
        newRoom.Initialize( data );

        rooms.AddLast( newRoom );
    }

    private void AckDeleteRoomInfo( Packet _packet )
    {
        var data = Global.FromJson<ROOM_INFO>( _packet );

        var node = FindRoomInfoNode( data.uid );
        if ( node == null )
             return;

        rooms.Remove( node );
    }
    #endregion
    #endregion
}

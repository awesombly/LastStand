using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;


using static PacketType;
public struct RoomData
{
    public ushort uid;
    public string title;
    public int maxPersonnel;

    public RoomData( ushort _uid, string _title, int _maxpersonnel )
    {
        uid = _uid;
        title = _title;
        maxPersonnel = _maxpersonnel;
    }
}

public class RoomSystem : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField title;
    public Transform contents;
    public Room prefab;
    
    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendMakeRoom = true;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( CREATE_ROOM_RES, ResCreateRoom );
        ProtocolSystem.Inst.Regist( TAKE_ROOM_LIST,  ResTakeRoom );
    }

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

    private void ResCreateRoom( Packet _packet )
    {
        canSendMakeRoom = true;
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            // ���� ����
        }
        else
        {
            // ���� �޼���
        }
    }

    private void ResTakeRoom( Packet _packet )
    {
        // Room room = Instantiate( prefab, contents as RectTransform );
        // var data = Global.FromJson<ResTakeRoom>( _packet );
        // room.Initialize( data.room );

        //List<RoomData> rooms = new List<RoomData>()
        //{
        //    new RoomData(1,"fslkafjksla", 100),
        //    new RoomData(2,"sfdSFKDOPK", 200),
        //    new RoomData(3,"FSDOPFMOP", 300),
        //};

        //var rooms = JsonConvert.DeserializeObject<ResTakeRoom>( System.Text.Encoding.UTF8.GetString( _packet.data ) );
        //foreach ( var room in rooms.rooms )
        //    Debug.Log( $"{room.uid}  {room.title}  {room.maxPersonnel}" );

        //Debug.Log( JsonConvert.SerializeObject( rooms, Formatting.None ) );
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    
    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendMakeRoom = true;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( new ResMakeRoom(), ResponseMakeRoom );
    }

    public void ShowMakeRoomPanel( bool _active )
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

    public void MakeRoom()
    {
        if ( !canSendMakeRoom )
             return;

        canSendMakeRoom = false;
        ReqMakeRoom protocol;
        protocol.title = title.text;
        protocol.maxPersonnel = maxPersonnel;

        Network.Inst.Send( new Packet( protocol ) );
    }

    private void ResponseMakeRoom( Packet _packet )
    {
        canSendMakeRoom = true;
        var data = Global.FromJson<ResMakeRoom>( _packet );
        if ( data.isCompleted )
        {
            // 게임 접속
            GameManager.Inst.roomData = new RoomData( data.uid, title.text, maxPersonnel );
        }
        else
        {
            // 실패 메세지
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using static PacketType;

public class StageSystem : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField title;
    public Transform contents;
    public Stage prefab;
    
    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendMakeStage = true;

    public LinkedList<Stage> stages = new LinkedList<Stage>();

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( CREATE_STAGE_ACK,  AckCreateStage );
        ProtocolSystem.Inst.Regist( LOBBY_INFO_ACK,    AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( INSERT_STAGE_INFO, AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( DELETE_STAGE_INFO, AckDeleteStageInfo );
        ProtocolSystem.Inst.Regist( UPDATE_STAGE_INFO, AckUpdateStageInfo );
    }

    private void Start()
    {
        if ( Network.Inst.IsConnected )
        {
            Network.Inst.Send( new Packet( LOBBY_INFO_REQ, new EMPTY() ) );
        }
    }

    #region Button Events
    public void ShowCreateStagePanel( bool _active )
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

    public void CreateStage()
    {
        if ( !canSendMakeStage )
             return;

        canSendMakeStage = false;
        STAGE_INFO protocol;
        protocol.serial    = 0;
        protocol.title     = title.text;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };
        
        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
    }
    #endregion

    #region Protocols
    private void AckCreateStage( Packet _packet )
    {
        canSendMakeStage = true;
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

    //// 로비에 처음 입장 시 서버에 등록된 방 정보 갱신
    //private void AckLobbyInfo( Packet _packet )
    //{
    //    var data = Global.FromJson<LOBBY_INFO>( _packet );
    //    foreach ( var info in data.infos )
    //    {
    //        // 풀링하기
    //        Stage stage = Instantiate( prefab, contents );
    //        stage.Initialize( info );

    //        stages.AddLast( stage );
    //    }
    //}

    private LinkedListNode<Stage> FindStageInfoNode( ushort _seiral )
    {
        for ( var node = stages.First; node != null; node = node.Next )
        {
            var stage = node.Value;
            if ( stage.info.serial == _seiral )
                 return node;
        }

        return null;
    }

    #region Update Stage Info
    private void AckUpdateStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );

        var node = FindStageInfoNode( data.serial );
        if ( node == null )
             return;

        node.Value.Initialize( data );
    }

    private void AckInsertStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );
        Stage newStage = Instantiate( prefab, contents );
        newStage.Initialize( data );

        stages.AddLast( newStage );
    }

    private void AckDeleteStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );

        var node = FindStageInfoNode( data.serial );
        if ( node == null )
             return;

        stages.Remove( node );
    }
    #endregion
    #endregion
}

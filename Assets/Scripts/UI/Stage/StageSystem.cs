using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using static PacketType;
using UnityEngine.SceneManagement;

public class StageSystem : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField title;
    public Transform contents;
    public Stage prefab;
    
    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendCreateStage = true;

    public LinkedList<Stage> stages = new LinkedList<Stage>();

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( CREATE_STAGE_ACK,  AckCreateStage );
        ProtocolSystem.Inst.Regist( ENTRY_STAGE_ACK,   AckEntryStage );
        ProtocolSystem.Inst.Regist( STAGE_INFO_ACK,    AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( INSERT_STAGE_INFO, AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( DELETE_STAGE_INFO, AckDeleteStageInfo );
        ProtocolSystem.Inst.Regist( UPDATE_STAGE_INFO, AckUpdateStageInfo );
    }

    private void Start()
    {
        if ( Network.Inst.IsConnected )
        {
            Network.Inst.Send( new Packet( STAGE_INFO_REQ, new EMPTY() ) );
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
        if ( !canSendCreateStage )
             return;

        canSendCreateStage = false;
        STAGE_INFO protocol;
        protocol.serial    = 0;
        protocol.title     = title.text;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };
        
        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
    }
    #endregion

    #region Protocols
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

    private void AckCreateStage( Packet _packet )
    {
        canSendCreateStage = true;
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            SceneManager.LoadScene( "SampleScene" );
        }
        else
        {
            // 실패 메세지
        }
    }

    private void AckEntryStage( Packet _packet )
    {
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            SceneManager.LoadScene( "SampleScene" );
        }
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

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static PacketType;

public class StageSystem : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField title;
    public Transform contents;
    public Stage prefab;
    private WNS.ObjectPool<Stage> pool;

    public List<Outline> personnelOutlines = new List<Outline>();
    private int maxPersonnel;
    private bool canSendCreateStage = true;

    public static STAGE_INFO? Info { get; set; }
    public List<Stage> stages = new List<Stage>();

    private void Awake()
    {
        var net = Network.Inst;
        pool = new WNS.ObjectPool<Stage>( prefab, contents );

        ProtocolSystem.Inst.Regist( STAGE_INFO_ACK,    AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( INSERT_STAGE_INFO, AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( CREATE_STAGE_ACK,  AckEntryStage );
        ProtocolSystem.Inst.Regist( ENTRY_STAGE_ACK,   AckEntryStage );
        ProtocolSystem.Inst.Regist( UPDATE_STAGE_INFO, AckUpdateStageInfo );
        ProtocolSystem.Inst.Regist( DELETE_STAGE_INFO, AckDeleteStageInfo );
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

    public void ExitGame() => Application.Quit();
    #endregion

    #region Protocols
    private void AckEntryStage( Packet _packet )
    {
        canSendCreateStage = true;
        Info = Global.FromJson<STAGE_INFO>( _packet );
        SceneBase.ChangeScene( SceneType.InGame );
    }

    #region Update Stage Info
    private void AckUpdateStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );
        foreach ( var stage in stages )
        {
            if ( stage.info.serial == data.serial )
            {
                stage.Initialize( data );
                return;
            }
        }
    }

    private void AckInsertStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );

        Stage newStage = pool.Spawn();
        newStage.Initialize( data );

        stages.Add( newStage );
    }

    private void AckDeleteStageInfo( Packet _packet )
    {
        var data = Global.FromJson<STAGE_INFO>( _packet );
        foreach ( var stage in stages )
        {
            if ( stage.info.serial == data.serial )
            {
                stages.Remove( stage );
                pool.Despawn( stage );
                return;
            }
        }
    }
    #endregion
    #endregion
}

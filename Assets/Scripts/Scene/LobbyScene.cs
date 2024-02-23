using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using static PacketType;

public class LobbyScene : SceneBase
{
    [Header( "< Create Stage >" )]
    public GameObject createStageCanvas;
    public TMP_InputField title;

    public List<Outline> personnelOutlines  = new List<Outline>();
    public List<Outline> targetKillOutlines = new List<Outline>();
    private int maxPersonnel;
    private int targetKillCount;

    [Header( "< Current Stage List >" )]
    public List<Stage> stages = new List<Stage>();
    private WNS.ObjectPool<Stage> pool;
    public Transform contents;
    public Stage prefab;

    private bool canSendCreateStage = true;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Lobby;

        var net = Network.Inst;
        pool = new WNS.ObjectPool<Stage>( prefab, contents );

        ProtocolSystem.Inst.Regist( STAGE_INFO_ACK,    AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( INSERT_STAGE_INFO, AckInsertStageInfo );
        ProtocolSystem.Inst.Regist( CREATE_STAGE_ACK,  AckEntryStage );
        ProtocolSystem.Inst.Regist( ENTRY_STAGE_ACK,   AckEntryStage );
        ProtocolSystem.Inst.Regist( UPDATE_STAGE_INFO, AckUpdateStageInfo );
        ProtocolSystem.Inst.Regist( DELETE_STAGE_INFO, AckDeleteStageInfo );

        StartCoroutine( WaitForAudioLoad() );
    }

    protected override void Start()
    {
        base.Start();
        if ( Network.Inst.IsConnected )
        {
            Network.Inst.Send( new Packet( STAGE_INFO_REQ, new EMPTY() ) );
        }
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGM.Lobby, 0f, .5f, 5f );
    }
    #endregion

    #region Create Stage
    #region Button Events
    public void ShowCreateStagePanel( bool _active )
    {
        if ( _active )
        {
            if ( createStageCanvas.activeInHierarchy )
                return;

            title.ActivateInputField();
            SetPersonnel( 4 );
            SetTargetKillCount( 40 );
            AudioManager.Inst.Play( SFX.MenuEntry );
        }
        else
        {
            title.text = string.Empty;
            title.DeactivateInputField();
            AudioManager.Inst.Play( SFX.MenuExit );
        }

        createStageCanvas.SetActive( _active );
    }

    public void SetPersonnel( int _max )
    {
        maxPersonnel = _max;
        for ( int i = 0; i < personnelOutlines.Count; i++ )
        {
            personnelOutlines[i].enabled = i == _max - 1 ? true : false;
        }
    }

    public void SetTargetKillCount( int _killCount )
    {
        targetKillCount = _killCount;
        for ( int i = 0; i < targetKillOutlines.Count; i++ )
        {
            targetKillOutlines[i].enabled = i == Global.Mathematics.Round( _killCount * .1f ) - 1 ? true : false;
        }
    }

    public void CreateStage()
    {
        if ( !canSendCreateStage )
            return;

        canSendCreateStage = false;
        STAGE_INFO protocol;
        protocol.serial = 0;
        protocol.title = title.text;
        protocol.targetKill = targetKillCount;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };

        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    public void ExitGame() => Application.Quit();
    #endregion

    #region Protocols
    private void AckEntryStage( Packet _packet )
    {
        canSendCreateStage = true;
        GameManager.StageInfo = Global.FromJson<STAGE_INFO>( _packet );
        SceneBase.LoadScene( SceneType.InGame_UI );
        SceneBase.LoadScene( SceneType.InGame_Logic, LoadSceneMode.Additive );
    }

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

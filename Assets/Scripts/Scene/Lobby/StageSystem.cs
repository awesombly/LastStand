using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

using static PacketType;
public class StageSystem : MonoBehaviour
{
    private SceneBase scene;
    [Header( "< Create Stage >" )]
    public GameObject createStageCanvas;
    public CanvasGroup createStageGroup;
    public TMP_InputField title;
    private bool isStageFadePlaying;

    public List<ButtonActivator> personnels  = new List<ButtonActivator>();
    public List<ButtonActivator> targetKills = new List<ButtonActivator>();
    private int maxPersonnel;
    private int targetKillCount;

    [Header( "< Current Stage List >" )]
    public List<Stage> stages = new List<Stage>();
    private WNS.ObjectPool<Stage> pool;
    public Transform contents;
    public Stage prefab;

    #region Unity Callback
    private void Awake()
    {
        TryGetComponent( out scene );

        var net = Network.Inst;
        pool = new WNS.ObjectPool<Stage>( prefab, contents );
        
        // Protocols
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
             Network.Inst.Send( new Packet( STAGE_INFO_REQ ) );
    }
    #endregion

    #region Button Events
    public void ShowCreateStagePanel()
    {
        if ( isStageFadePlaying )
             return;

        if ( createStageCanvas.activeInHierarchy )
        {
            title.text = string.Empty;
            title.DeactivateInputField();
            AudioManager.Inst.Play( SFX.MenuExit );

            isStageFadePlaying = true;
            createStageGroup.DOFade( 0f, .25f ).OnComplete( () =>
            {
                isStageFadePlaying = false;
                createStageCanvas.SetActive( false );
            } );
        }
        else
        {
            createStageCanvas.SetActive( true );
            title.ActivateInputField();
            SetPersonnel( 4 );
            SetTargetKillCount( 40 );
            personnels[3].Enabled = true;
            targetKills[3].Enabled = true;

            AudioManager.Inst.Play( SFX.MenuEntry );
            createStageGroup.alpha = 0f;

            isStageFadePlaying = true;
            createStageGroup.DOFade( 1f, .25f ).OnComplete( () => { isStageFadePlaying = false; } );
        }
    }

    public void SetPersonnel( int _max )
    {
        maxPersonnel = _max;
        for ( int i = 0; i < personnels.Count; i++ )
            personnels[i].Enabled = false;
    }

    public void SetTargetKillCount( int _killCount )
    {
        targetKillCount = _killCount;
        for ( int i = 0; i < targetKills.Count; i++ )
            targetKills[i].Enabled = false;
    }

    public void CreateStage()
    {
        if ( SceneBase.IsLock )
             return;

        SceneBase.IsLock = true;
        STAGE_INFO protocol;
        protocol.serial = 0;
        protocol.title = title.text;
        protocol.targetKill = targetKillCount;
        protocol.currentKill = 0;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };

        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
    }

    public void ExitGame() => Application.Quit();
    #endregion

    #region Response Protocols
    private void AckEntryStage( Packet _packet )
    {
        switch ( _packet.result )
        {
            case Result.OK:
            {
                GameManager.StageInfo = Global.FromJson<STAGE_INFO>( _packet );
                scene.LoadScene( SceneType.InGame_UI );
                scene.LoadScene( SceneType.InGame_Logic, LoadSceneMode.Additive );
                AudioManager.Inst.Play( SFX.MouseClick );
            } break;

            default:
            {
                SceneBase.IsLock = false;
            } break;
        }
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

using static PacketType;
public class InGameUIScene : SceneBase
{
    public Camera uiCamera;

    [Header( "< Mouse >" )]
    public Transform cursor;

    [Header( "< Magaginze >" )]
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    [Header( "< Target Kill >" )]
    public TextMeshProUGUI targetKillCount;
    private int remainedKillCount;

    [Header( "< Pause >" )]
    public GameObject pause;

    [Header( "< Player Select >" )]
    public GameObject selectPlayer;

    [Header( "< Player Dead >" )]
    public PlayerDeadUI deadPrefab;
    public Transform deadContents;
    private WNS.ObjectPool<PlayerDeadUI> deadUIPool;

    [Header( "< Virtual Pad >" )]
    public GameObject virtualPad;

    private bool canExitStage = true;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType           = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;
        IsLock              = true;

        deadUIPool = new WNS.ObjectPool<PlayerDeadUI>( deadPrefab, deadContents );

        if ( GameManager.StageInfo is not null )
        {
            remainedKillCount = GameManager.StageInfo.Value.targetKill - GameManager.StageInfo.Value.currentKill;
            targetKillCount.text = $"{remainedKillCount}";
        }

        GameManager.OnDead     += OnPlayerDead;

        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK,  AckExitStage );
        ProtocolSystem.Inst.Regist( CHANGE_HOST_ACK, AckChangeHost );

#if UNITY_ANDROID || UNITY_IOS
        cursor.gameObject.SetActive( false );
        virtualPad.SetActive( true );
#else
        virtualPad.SetActive( false );
#endif
    }

    protected override void Start()
    {
        base.Start();

        selectPlayer.SetActive( true );
    }

    private void Update()
    {
        if ( cursor.gameObject.activeSelf )
        {
            Vector3 pos = Input.mousePosition;
            cursor.position = uiCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, 10f ) );
        }

        if ( IsLock )
             return;

        if ( Input.GetKeyDown( KeyCode.Escape ) )
             ActivePause();
    }

    private void OnDestroy()
    {
        GameManager.OnDead -= OnPlayerDead;
    }
    #endregion

    public void ActivePause()
    {
        if ( pause.activeInHierarchy )
        {
            if ( GameManager.LocalPlayer is not null )
                 GameManager.LocalPlayer.UnmoveableCount--;

            pause.SetActive( false );
            AudioManager.Inst.Play( SFX.MenuExit );
        }
        else
        {
            if ( GameManager.LocalPlayer is not null )
                 GameManager.LocalPlayer.UnmoveableCount++;

            pause.SetActive( true );
            AudioManager.Inst.Play( SFX.MenuEntry );
        }
    }

    public void ReqExitStage()
    {
        if ( !canExitStage || GameManager.StageInfo is null )
             return;

        IsLock       = true;
        canExitStage = false;
        Network.Inst.Send( EXIT_STAGE_REQ, GameManager.StageInfo.Value );
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    public void ReqSelectPlayer( int _type )
    {
        IsLock = false;
        selectPlayer.SetActive( false );
        Player playerPrefab = GameManager.Inst.GetPlayerPrefab();
        var logicScene = GameManager.Inst.GetActiveScene( SceneType.InGame_Logic ) as InGameLogicScene;

        PLAYER_INFO protocol;
        protocol.actorInfo.isLocal = true;
        protocol.actorInfo.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
        protocol.actorInfo.hash = 0;
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.pos = new VECTOR2( logicScene.GetSpawnPosition() );
        protocol.actorInfo.vel = new VECTOR2( Vector2.zero );
        protocol.actorInfo.hp = playerPrefab.data.maxHp;
        protocol.actorInfo.inter = 0f;
        protocol.nickname = string.Empty;
        protocol.isDead = false;
        protocol.angle = 0f;
        protocol.weapon = 1;
        protocol.kill = 0;
        protocol.death = 0;
        protocol.type = ( PlayerType )_type;

        Network.Inst.Send( SPAWN_PLAYER_REQ, protocol );
    }

    private void AckExitStage( Packet _packet )
    {
        GameManager.StageInfo = null;
        LoadScene( SceneType.Lobby );
    }

    private void AckChangeHost( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );

        STAGE_INFO info = GameManager.StageInfo.Value;
        info.hostSerial = data.serial;
        GameManager.StageInfo = info;

        foreach ( var Player in GameManager.Players )
        {
            Player.Board.ActiveCrown( Player.Serial == info.hostSerial );
        }
    }

    private void OnPlayerDead( Player _player )
    {
        targetKillCount.text = $"{--remainedKillCount}";

        PlayerDeadUI deadUI = deadUIPool.Spawn();
        deadUI.Initialize( _player );
    }
}

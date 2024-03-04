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

    [Header( "< Result >" )]
    public GameObject gameResult;
    public TextMeshProUGUI resultText;
    public List<ResultBoard> resultBoards;

    public TextMeshProUGUI resultLevel;
    public Slider          resultExp;
    public GameObject      resultBackButton;

    [Header( "< Pause >" )]
    public GameObject pause;

    [Header( "< Player Select >" )]
    public GameObject selectPlayer;

    [Header( "< Player Dead >" )]
    public PlayerDeadUI deadPrefab;
    public Transform deadContents;
    private WNS.ObjectPool<PlayerDeadUI> deadUIPool;

    private bool canExitStage = true;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType           = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;
        IsLock              = true;

        deadUIPool = new WNS.ObjectPool<PlayerDeadUI>( deadPrefab, deadContents );

        if ( !ReferenceEquals( GameManager.StageInfo, null ) ) 
             targetKillCount.text = $"{GameManager.StageInfo.Value.targetKill}";

        GameManager.OnGameOver += OnGameOver;
        GameManager.OnDead     += OnPlayerDead;

        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK,         AckExitStage );
        ProtocolSystem.Inst.Regist( UPDATE_RESULT_INFO_ACK, AckUpdateResultInfo );
    }

    protected override void Start()
    {
        base.Start();

        selectPlayer.SetActive( true );
    }

    private void Update()
    {
        Vector3 pos = Input.mousePosition;
        cursor.position = uiCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, 10f ) );

        if ( IsLock )
             return;

        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if ( pause.activeInHierarchy )
            {
                InputSystem.EnableDevice( Mouse.current );
                InputSystem.EnableDevice( Keyboard.current );
                pause.SetActive( false );
                AudioManager.Inst.Play( SFX.MenuExit );
            }
            else
            {
                InputSystem.DisableDevice( Mouse.current );
                InputSystem.DisableDevice( Keyboard.current );
                pause.SetActive( true );
                AudioManager.Inst.Play( SFX.MenuEntry );
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameOver      -= OnGameOver;
        GameManager.OnDead          -= OnPlayerDead;
    }
    #endregion

    public void MoveToLobby()
    {
        AudioManager.Inst.Play( SFX.MouseClick );
        LoadScene( SceneType.Lobby );
    }

    public void ReqExitStage()
    {
        if ( IsLock || !canExitStage || GameManager.StageInfo == null )
             return;

        IsLock = true;
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
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.pos = new VECTOR2( logicScene.GetSpawnPosition() );
        protocol.actorInfo.vel = new VECTOR2( Vector2.zero );
        protocol.actorInfo.hp = playerPrefab.data.maxHp;
        protocol.actorInfo.index = 0;
        protocol.nickname = string.Empty;
        protocol.isDead = false;
        protocol.angle = 0f;
        protocol.weapon = 1;
        protocol.kill = 0;
        protocol.death = 0;
        protocol.type = ( PlayerType )_type;

        Network.Inst.Send( SPAWN_PLAYER_REQ, protocol );
    }

    private void AckUpdateResultInfo( Packet _packet )
    {
        var prevInfo = GameManager.UserInfo.Value;
        GameManager.UserInfo = Global.FromJson<USER_INFO>( _packet );
        var curInfo = GameManager.UserInfo.Value;

        StartCoroutine( SmoothLevelUp( prevInfo, curInfo ) );
    }

    private IEnumerator SmoothLevelUp( USER_INFO _prev, USER_INFO _cur )
    {
        float time = 0f;
        float prevExp = ( _prev.exp / Global.GetTotalEXP( _prev.level ) );
        float curExp  = ( _cur.exp  / Global.GetTotalEXP( _cur.level ) );
        bool isLevelUp = false;

        while ( true )
        {

            if ( _prev.level < _cur.level )
            {
                resultExp.value = WNS.Math.Lerp( prevExp, 1f, time );
                time += ( 1f + ( _cur.level - _prev.level ) ) * Time.deltaTime;

                if ( time >= 1f )
                {
                    isLevelUp = true;

                    time             = 0f;
                    _prev.level     += 1;
                    _prev.exp        = 0f;
                    prevExp          = ( _prev.exp / Global.GetTotalEXP( _prev.level ) );
                    resultExp.value  = 0f;
                    resultLevel.text = $"{_prev.level}";
                }
            }
            else
            {
                resultExp.value = isLevelUp ? WNS.Math.Lerp( 0f,      curExp, time ) :
                                              WNS.Math.Lerp( prevExp, curExp, time );

                time += Time.deltaTime;
                if ( time >= 1f )
                {
                    resultExp.value = curExp;
                    break;
                }
            }

            yield return null;
        }

        resultBackButton.SetActive( true );
    }

    private void AckExitStage( Packet _packet )
    {
        IsLock = canExitStage = false;
        GameManager.StageInfo = null;
        LoadScene( SceneType.Lobby );
    }

    private void OnPlayerDead( Player _player )
    {
        PlayerDeadUI deadUI = deadUIPool.Spawn();
        deadUI.Initialize( _player );
    }

    private void OnGameOver( Player _winner )
    {
        gameResult.SetActive( true );
        bool isWinner = ReferenceEquals( GameManager.LocalPlayer, _winner );
        resultText.text = isWinner ? "- ½Â¸® -" : "- ÆÐ¹è -";

        var players = GameManager.Players;
        for ( int i = 0; i < resultBoards.Count; ++i )
        {
            if ( players.Count <= i )
            {
                resultBoards[i].gameObject.SetActive( false );
                continue;
            }

            resultBoards[i].gameObject.SetActive( true );
            resultBoards[i].Initialize( players[i], ReferenceEquals( players[i], _winner ) );
        }

        if ( GameManager.UserInfo != null )
        {
            var userInfo = GameManager.UserInfo.Value;
            resultLevel.text = $"{userInfo.level}";
            resultExp.value  = userInfo.exp / Global.GetTotalEXP( userInfo.level );
        }

        if ( GameManager.LoginInfo != null )
        {
            RESULT_INFO protocol;
            protocol.uid   = GameManager.LoginInfo.Value.uid;
            protocol.kill  = GameManager.LocalPlayer.KillScore;
            protocol.death = GameManager.LocalPlayer.DeathScore;
            Network.Inst.Send( new Packet( UPDATE_RESULT_INFO_REQ, protocol ) );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using DG.Tweening;

using static PacketType;

public class InGameUIScene : SceneBase
{
    public Camera uiCamera;

    [Header( "< Mouse >" )]
    public RectTransform mouseCursor;

    [Header( "< Magaginze >" )]
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    [Header( "< Target Kill >" )]
    public TextMeshProUGUI targetKillCount;

    [Header( "< Player Board >" )]
    public List<PlayerBoard> boards = new List<PlayerBoard>();

    [Header( "< Result >" )]
    public GameObject gameResult;
    public TextMeshProUGUI resultText;
    public List<ResultBoard> resultBoards;

    public TextMeshProUGUI resultLevel;
    public Slider          resultExp;
    public GameObject      resultBackButton;

    [Header( "< Pause >" )]
    public GameObject pause;

    [Header( "< Player Dead >" )]
    public PlayerDeadUI deadPrefab;
    public Transform deadContents;
    private WNS.ObjectPool<PlayerDeadUI> deadUIPool;

    private bool canExitStage = true;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;

        deadUIPool = new WNS.ObjectPool<PlayerDeadUI>( deadPrefab, deadContents );

        if ( !ReferenceEquals( GameManager.StageInfo, null ) ) 
             targetKillCount.text = $"{GameManager.StageInfo.Value.targetKill}";

        GameManager.OnChangePlayers += UpdatePlayerBoard;
        GameManager.OnGameOver      += OnGameOver;
        GameManager.OnDead          += OnPlayerDead;

        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK,         AckExitStage );
        ProtocolSystem.Inst.Regist( UPDATE_RESULT_INFO_ACK, AckUpdateResultInfo );
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        mouseCursor.anchoredPosition = Mouse.current.position.value;

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
        GameManager.OnChangePlayers -= UpdatePlayerBoard;
        GameManager.OnGameOver      -= OnGameOver;
        GameManager.OnDead          -= OnPlayerDead;
    }
    #endregion

    private void AckUpdateResultInfo( Packet _packet )
    {
        resultBackButton.SetActive( true );

        var prevInfo = GameManager.UserInfo.Value;
        GameManager.UserInfo = Global.FromJson<USER_INFO>( _packet );
        
        var curInfo = GameManager.UserInfo.Value;
        if ( prevInfo.level < curInfo.level )
        {
            resultExp.DOValue( 1f, .5f ).OnComplete( () =>
            {
                resultExp.value = 0f;
                resultExp.DOValue( curInfo.exp / Global.GetTotalEXP( curInfo.level ), .5f );
            } );
        }
        else
        {
            resultExp.DOValue( curInfo.exp / Global.GetTotalEXP( curInfo.level ), .5f );
        }
    }

    public void MoveToLobby()
    {
        AudioManager.Inst.Play( SFX.MouseClick );
        LoadScene( SceneType.Lobby );
    }

    private void OnPlayerDead( Player _player )
    {
        PlayerDeadUI deadUI = deadUIPool.Spawn();
        deadUI.Initialize( _player );
    }

    private void UpdatePlayerBoard()
    {
        var players = GameManager.Players;
        for ( int i = 0; i < 4; i++ )
        {
            if ( i < players.Count )
            {
                boards[i].gameObject.SetActive( true );
                boards[i].Initialize( players[i] );
            }
            else
            {
                boards[i].gameObject.SetActive( false );
            }
        }
    }

    public void ReqExitStage()
    {
        if ( IsSending || !canExitStage || GameManager.StageInfo == null )
            return;

        IsSending = true;
        Network.Inst.Send( EXIT_STAGE_REQ, GameManager.StageInfo.Value );
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    private void AckExitStage( Packet _packet )
    {
        IsSending = canExitStage = false;
        GameManager.StageInfo = null;
        LoadScene( SceneType.Lobby );
    }

    private void OnGameOver( Player _winner )
    {
        gameResult.SetActive( true );
        bool isWinner = ReferenceEquals( GameManager.LocalPlayer, _winner );
        resultText.text = isWinner ? "- ½Â¸® -" : "- ÆÐ¹è -";

        var players = GameManager.Players;
        for ( int i = 0; i < resultBoards.Count; ++i )
        {
            if ( i >= players.Count )
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

        RESULT_INFO protocol;
        protocol.uid   = GameManager.LoginInfo.Value.uid;
        protocol.kill  = GameManager.LocalPlayer.KillScore;
        protocol.death = GameManager.LocalPlayer.DeathScore;
        Network.Inst.Send( new Packet( UPDATE_RESULT_INFO_REQ, protocol ) );
    }
}

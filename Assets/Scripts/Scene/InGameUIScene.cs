using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

using static PacketType;

public class InGameUIScene : SceneBase
{
    public Camera uiCamera;

    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    public TextMeshProUGUI targetKillCount;

    public List<PlayerBoard> boards = new List<PlayerBoard>();

    // Game Result
    public GameObject gameResult;
    public TextMeshProUGUI resultText;
    public List<ResultBoard> resultBoards;

    public GameObject pause;
    private bool isProgress;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;

        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK, AckExitStage );

        if ( !ReferenceEquals( GameManager.StageInfo, null ) ) 
             targetKillCount.text = $"{GameManager.StageInfo.Value.targetKill}";

        GameManager.OnChangePlayers += UpdatePlayerBoard;
        GameManager.OnGameOver += OnGameOver;
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
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
        GameManager.OnGameOver -= OnGameOver;
    }
    #endregion

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
        if ( isProgress || GameManager.StageInfo == null )
            return;

        isProgress = true;
        Network.Inst.Send( EXIT_STAGE_REQ, GameManager.StageInfo.Value );
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    private void AckExitStage( Packet _packet )
    {
        isProgress = false;
        GameManager.StageInfo = null;
        LoadScene( SceneType.Lobby );
    }

    private void OnGameOver( Player _winner )
    {
        gameResult.SetActive( true );
        bool isWinner = ReferenceEquals( GameManager.LocalPlayer, _winner );
        resultText.text = isWinner ? "- �¸� -" : "- �й� -";

        var players = GameManager.Players;
        for ( int i = 0; i < resultBoards.Count; ++i )
        {
            if ( i >= players.Count )
            {
                resultBoards[i].gameObject.SetActive( false );
                return;
            }

            resultBoards[i].gameObject.SetActive( true );
            resultBoards[i].Initialize( players[i], ReferenceEquals( players[i], _winner ) );
        }
    }
}

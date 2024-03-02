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
    private bool canCreateStage = true;

    [Header( "< Current Stage List >" )]
    public List<Stage> stages = new List<Stage>();
    private WNS.ObjectPool<Stage> pool;
    public Transform contents;
    public Stage prefab;

    [Header( "===================================================" )]
    [Header( "< Option >" )]
    public GameObject optionCanvas;

    [Header( "< UserInfo >" )]
    public GameObject userInfoCanvas;

    [Header( "===================================================" )]
    [Header( "< Login >" )]
    public GameObject loginCanvas;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField nickname;

    [Header( "< Login Error >" )]
    public GameObject      errorPanel;
    public TextMeshProUGUI errorMessage;

    [Header( "< Account >" )]
    public GameObject      signUpPanel;
    public TextMeshProUGUI signUpMessage;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Lobby;

        var net = Network.Inst;
        pool = new WNS.ObjectPool<Stage>( prefab, contents );

        // Login
        ProtocolSystem.Inst.Regist( CONFIRM_LOGIN_ACK,   AckConfirmMatchData );
        ProtocolSystem.Inst.Regist( CONFIRM_ACCOUNT_ACK, AckAddAccountInfoToDB );
        ProtocolSystem.Inst.Regist( DUPLICATE_EMAIL_ACK, AckConfirmDuplicateEmail );

        // Stage
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
             Network.Inst.Send( new Packet( STAGE_INFO_REQ, new EMPTY() ) );
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGM.Lobby, 0f, .5f, 5f );
    }
    #endregion

    #region Login
    #region Button Events
    public void ActiveErrorPanel( bool _isActive )
    {
        errorPanel.SetActive( _isActive );
        if ( _isActive )
        {
            password.DeactivateInputField();
            email.DeactivateInputField();
            nickname.DeactivateInputField();
        }
        else email.ActivateInputField();
    }

    public void ActiveSignUpPanel( bool _isActive )
    {
        signUpPanel.SetActive( _isActive );
        if ( _isActive ) nickname.ActivateInputField();
        else             email.ActivateInputField();
    }

    public void ActiveOptionPanel()
    {
        if ( optionCanvas.activeInHierarchy )
        {
            optionCanvas.SetActive( false );
            AudioManager.Inst.Play( SFX.MenuExit );
        }
        else
        {
            optionCanvas.SetActive( true );

            email?.ActivateInputField();
            if ( password != null )
                 password.contentType = TMP_InputField.ContentType.Password;

            AudioManager.Inst.Play( SFX.MenuEntry );
        }
    }

    #region Request Protocols
    public void ReqConfirmLoginInfo()
    {
        LOGIN_INFO protocol;
        protocol.uid      = -1;
        protocol.email    = email.text;
        protocol.password = password.text;
        protocol.nickname = string.Empty;
        Network.Inst.Send( new Packet( CONFIRM_LOGIN_REQ, protocol ) );
    }

    public void ReqConfirmDuplicateEmail()
    {
        LOGIN_INFO protocol;
        protocol.uid      = -1;
        protocol.email    = email.text;
        protocol.password = password.text;
        protocol.nickname = string.Empty;
        Network.Inst.Send( new Packet( DUPLICATE_EMAIL_REQ, protocol ) );
    }

    public void ReqConfirmAccountInfo()
    {
        if ( nickname.text == string.Empty )
             return;

        LOGIN_INFO protocol;
        protocol.uid      = -1;
        protocol.nickname = nickname.text;
        protocol.email    = email.text;
        protocol.password = password.text;

        Network.Inst.Send( new Packet( CONFIRM_ACCOUNT_REQ, protocol ) );
    }
    #endregion
    #endregion

    #region Response Protocols
    private void AckConfirmDuplicateEmail( Packet _packet )
    {
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            ActiveSignUpPanel( true );
            signUpMessage.text = string.Empty;
            nickname.text = string.Empty;
        }
        else
        {
            ActiveErrorPanel( true );
            errorMessage.text = "중복된 아이디 입니다.";
        }
    }

    public void AckAddAccountInfoToDB( Packet _packet )
    {
        if ( Global.FromJson<CONFIRM>( _packet ).isCompleted )
        {
            signUpPanel.SetActive( false );
            AudioManager.Inst.Play( SFX.MenuExit );
            ReqConfirmLoginInfo();
        }
    }

    private void AckConfirmMatchData( Packet _packet )
    {
        var data = Global.FromJson<ACCOUNT_INFO>( _packet );
        if ( data.loginInfo.nickname == string.Empty )
        {
            ActiveErrorPanel( true );
            errorMessage.text = "아이디 또는 비밀번호가 일치하지 않습니다.";
        }
        else
        {
            GameManager.LoginInfo = data.loginInfo;
            GameManager.UserInfo  = data.userInfo;

            loginCanvas.SetActive( false );
            userInfoCanvas.SetActive( true );
        }
    }
    #endregion
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
            targetKillOutlines[i].enabled = i == WNS.Math.Round( _killCount * .1f ) - 1 ? true : false;
        }
    }

    public void CreateStage()
    {
        if ( IsSending || !canCreateStage )
             return;

        IsSending = true;
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

    #region Response Protocols
    private void AckEntryStage( Packet _packet )
    {
        IsSending = canCreateStage = false;
        GameManager.StageInfo = Global.FromJson<STAGE_INFO>( _packet );
        LoadScene( SceneType.InGame_UI );
        LoadScene( SceneType.InGame_Logic, LoadSceneMode.Additive );
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

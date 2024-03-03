using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

using static PacketType;

public class LobbyScene : SceneBase
{
    public Camera uiCamera;

    [Header( "< Create Stage >" )]
    public GameObject createStageCanvas;
    public CanvasGroup createStageGroup;
    private Tween createStageGroupEffect;
    public TMP_InputField title;

    public List<ButtonActivator> personnels  = new List<ButtonActivator>();
    public List<ButtonActivator> targetKills = new List<ButtonActivator>();
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
    public RectTransform optionMovement;
    private Tween curOptionMoveTween;

    [Header( "< UserInfo >" )]
    public GameObject userInfoCanvas;
    public TextMeshProUGUI userInfoNickname;
    public TextMeshProUGUI level;
    public Slider exp;

    public TextMeshProUGUI playCount;
    public TextMeshProUGUI kill, death;
    public TextMeshProUGUI killDeathAverage;
    public TextMeshProUGUI bestKill, bestDeath;

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

    public Transform cursor;

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

        if ( GameManager.LoginInfo == null )
        {
            loginCanvas.SetActive( true );
            userInfoCanvas.SetActive( false );

            if ( password != null )
                 password.contentType = TMP_InputField.ContentType.Password;
        }
        else
        {
            loginCanvas.SetActive( false );
            userInfoCanvas.SetActive( true );
            UpdateUserInfo();
        }
    }

    protected override void Start()
    {
        base.Start();
        if ( Network.Inst.IsConnected )
             Network.Inst.Send( new Packet( STAGE_INFO_REQ, new EMPTY() ) );
    }
    private void Update()
    {
        Vector3 pos = Input.mousePosition;
        cursor.position = uiCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, 10f ) );
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGM.Lobby, 0f, .5f, 5f );
    }

    private void UpdateUserInfo()
    {
        if ( GameManager.LoginInfo == null || GameManager.UserInfo == null )
             return;

        LOGIN_INFO loginData = GameManager.LoginInfo.Value;
        userInfoNickname.text = $"- {loginData.nickname} -";
        
        USER_INFO userData = GameManager.UserInfo.Value;
        level.text            = $"{userData.level}";
        exp.value             = ( float )( userData.exp / Global.GetTotalEXP( userData.level ) ) ;
        playCount.text        = $"{userData.playCount}";
        kill.text             = $"{userData.kill}";
        death.text            = $"{userData.death}";

        float kd = ( ( userData.kill * 100f ) / ( userData.kill + userData.death ) );
        killDeathAverage.text = $"{( float.IsNaN( kd ) ? 0 : kd.ToString( "F1" ) )}";

        bestKill.text         = $"{userData.bestKill}";
        bestDeath.text        = $"{userData.bestDeath}";
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
        if ( curOptionMoveTween != null && curOptionMoveTween.IsPlaying() )
             return;

        if ( optionCanvas.activeInHierarchy )
        {
            AudioManager.Inst.Play( SFX.MenuExit );
            curOptionMoveTween = optionMovement.DOAnchorPosX( -325f, .5f ).OnComplete(() => optionCanvas.SetActive( false ) );
        }
        else
        {
            optionCanvas.SetActive( true );

            email?.ActivateInputField();
            if ( password != null )
                 password.contentType = TMP_InputField.ContentType.Password;

            curOptionMoveTween = optionMovement.DOAnchorPosX( 325f, .5f );
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
            UpdateUserInfo();
        }
    }
    #endregion
    #endregion

    #region Create Stage
    #region Button Events
    public void ShowCreateStagePanel()
    {
        if ( createStageGroupEffect != null && createStageGroupEffect.IsPlaying() )
             return;

        if ( !createStageCanvas.activeInHierarchy )
        {
            createStageCanvas.SetActive( true );
            title.ActivateInputField();
            SetPersonnel( 4 );
            SetTargetKillCount( 40 );
            personnels[3].Enabled  = true;
            targetKills[3].Enabled = true;

            AudioManager.Inst.Play( SFX.MenuEntry );
            createStageGroup.alpha = 0f;
            createStageGroup.DOFade( 1f, .25f );
        }
        else
        {
            title.text = string.Empty;
            title.DeactivateInputField();
            AudioManager.Inst.Play( SFX.MenuExit );
            createStageGroup.DOFade( 0f, .25f ).OnComplete( () => createStageCanvas.SetActive( false ) );
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
        if ( IsSending || !canCreateStage )
             return;

        IsSending = true;
        STAGE_INFO protocol;
        protocol.serial = 0;
        protocol.title = title.text;
        protocol.targetKill = targetKillCount;
        protocol.personnel = new Personnel { current = 0, maximum = maxPersonnel };

        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
    }

    public void ExitGame() => Application.Quit();
    #endregion

    #region Response Protocols
    private void AckEntryStage( Packet _packet )
    {
        IsSending = canCreateStage = false;
        AudioManager.Inst.Play( SFX.MouseClick );
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

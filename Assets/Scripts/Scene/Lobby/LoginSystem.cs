using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static PacketType;
public class LoginSystem : MonoBehaviour
{
    [Header( "< Login >" )]
    public GameObject loginCanvas;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField nickname;
    public Toggle remember;

    [Header( "< Login Error >" )]
    public GameObject      errorPanel;
    public TextMeshProUGUI errorMessage;

    [Header( "< Account >" )]
    public GameObject      signUpPanel;
    public TextMeshProUGUI signUpMessage;

    public event Action OnLoginCompleted;

    private void Awake()
    {
        // Protocols
        ProtocolSystem.Inst.Regist( CONFIRM_LOGIN_ACK,   AckConfirmMatchData );
        ProtocolSystem.Inst.Regist( CONFIRM_ACCOUNT_ACK, AckAddAccountInfoToDB );
        ProtocolSystem.Inst.Regist( DUPLICATE_EMAIL_ACK, AckConfirmDuplicateEmail );

        if ( GameManager.LoginInfo is null )
        {
            loginCanvas.SetActive( true );

            // 이벤트 설정
            if ( TryGetComponent( out OptionSystem optionSys ) )
                optionSys.OnActiveOption += () =>
                {
                    if ( email.text == string.Empty ) email.ActivateInputField();
                    else                              password.ActivateInputField();
                };

            // 패스워드 타입 설정( 코드로만 수정가능한 듯 )
            if ( password is not null )
                 password.contentType = TMP_InputField.ContentType.Password;

            // 저장된 ini 정보 읽기
            if ( bool.TryParse( Config.Inst.Read( ConfigType.isRemember ), out bool isRemember ) ) remember.isOn = isRemember;
            else                                                                                   Config.Inst.Write( ConfigType.isRemember, isRemember.ToString() );

            if ( remember.isOn )
            {
                email.text    = Config.Inst.Read( ConfigType.ID );
                //password.text = Config.Inst.Read( ConfigLogin.PW );
            }
        }
        else
        {
            loginCanvas.SetActive( false );
        }
    }

    public void RememberLoginInfo( bool _isRemember )
    {
        Config.Inst.Write( ConfigType.isRemember, _isRemember.ToString() );
        AudioManager.Inst.Play( SFX.MouseClick );
    }

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
        AudioManager.Inst.Play( SFX.MouseClick );
        if ( _isActive ) nickname.ActivateInputField();
        else             email.ActivateInputField();
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
        AudioManager.Inst.Play( SFX.MouseClick );
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
        protocol.email    = email.text;
        protocol.password = password.text;
        protocol.nickname = nickname.text;

        Network.Inst.Send( new Packet( CONFIRM_ACCOUNT_REQ, protocol ) );
    }
    #endregion
    #endregion

    #region Response Protocols
    private void AckConfirmDuplicateEmail( Packet _packet )
    {
        switch ( _packet.result )
        {
            case Result.DB_ERR_DISCONNECTED: break;
            case Result.OK:
            {
                ActiveSignUpPanel( true );
                signUpMessage.text = string.Empty;
                nickname.text      = string.Empty;
            }
            break;

            case Result.DB_ERR_DUPLICATE_DATA:
            {
                ActiveErrorPanel( true );
                errorMessage.text = "중복된 아이디 입니다.";
            }
            break;

            default:
            {
                Debug.LogWarning( _packet.result.ToString() );
            }
            break;
        }
    }

    public void AckAddAccountInfoToDB( Packet _packet )
    {
        if ( _packet.result == Result.OK )
        {
            signUpPanel.SetActive( false );
            AudioManager.Inst.Play( SFX.MenuExit );
            ReqConfirmLoginInfo();
        }
    }

    private void AckConfirmMatchData( Packet _packet )
    {
        switch( _packet.result )
        {
            case Result.DB_ERR_DISCONNECTED: break;
            case Result.OK:
            {
                var data = Global.FromJson<ACCOUNT_INFO>( _packet );
                GameManager.LoginInfo = data.loginInfo;
                GameManager.UserInfo  = data.userInfo;

                Config.Inst.Write( ConfigType.ID, email.text );
                Config.Inst.Write( ConfigType.PW, password.text );

                loginCanvas.SetActive( false );
                OnLoginCompleted?.Invoke();
            } break;

            default:
            {
                ActiveErrorPanel( true );
                errorMessage.text = "아이디 또는 비밀번호가 일치하지 않습니다.";
            } break;
        }
    }
    #endregion
}
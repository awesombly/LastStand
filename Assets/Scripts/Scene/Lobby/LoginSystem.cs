using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class LoginSystem : MonoBehaviour
{
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

    public event Action OnLoginCompleted;

    private void Awake()
    {
        // Protocols
        ProtocolSystem.Inst.Regist( CONFIRM_LOGIN_ACK,   AckConfirmMatchData );
        ProtocolSystem.Inst.Regist( CONFIRM_ACCOUNT_ACK, AckAddAccountInfoToDB );
        ProtocolSystem.Inst.Regist( DUPLICATE_EMAIL_ACK, AckConfirmDuplicateEmail );

        if ( GameManager.LoginInfo == null )
        {
            loginCanvas.SetActive( true );

            if ( password != null )
                 password.contentType = TMP_InputField.ContentType.Password;
        }
        else
        {
            loginCanvas.SetActive( false );
        }
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
        if ( _isActive ) nickname.ActivateInputField();
        else email.ActivateInputField();
    }

    #region Request Protocols
    public void ReqConfirmLoginInfo()
    {
        LOGIN_INFO protocol;
        protocol.uid = -1;
        protocol.email = email.text;
        protocol.password = password.text;
        protocol.nickname = string.Empty;
        Network.Inst.Send( new Packet( CONFIRM_LOGIN_REQ, protocol ) );
    }

    public void ReqConfirmDuplicateEmail()
    {
        LOGIN_INFO protocol;
        protocol.uid = -1;
        protocol.email = email.text;
        protocol.password = password.text;
        protocol.nickname = string.Empty;
        Network.Inst.Send( new Packet( DUPLICATE_EMAIL_REQ, protocol ) );
    }

    public void ReqConfirmAccountInfo()
    {
        if ( nickname.text == string.Empty )
            return;

        LOGIN_INFO protocol;
        protocol.uid = -1;
        protocol.nickname = nickname.text;
        protocol.email = email.text;
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
            GameManager.UserInfo = data.userInfo;

            loginCanvas.SetActive( false );
            OnLoginCompleted?.Invoke();

            //userInfoCanvas.SetActive( true );
            //UpdateUserInfo();
        }
    }
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


using static PacketType;
public class LoginSystem : MonoBehaviour
{
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField nickname;

[Header( "Panel" )]
    public GameObject      errorPanel;
    public TextMeshProUGUI errorMessage;

    public GameObject      signUpPanel;
    public TextMeshProUGUI signUpMessage;
    public GameObject      signUpExit;

    private enum LoginPanelType { Default, Error, SignUp, SignUpComplete }
    private LoginPanelType type = LoginPanelType.Default;

    private void Awake()
    {
        email?.ActivateInputField();

        if ( password != null )
             password.contentType = TMP_InputField.ContentType.Password;
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( CONFIRM_LOGIN_RES,   ResConfirmMatchData );
        ProtocolSystem.Inst.Regist( CONFIRM_SIGNUP_RES,  ResAddToDatabase );
        ProtocolSystem.Inst.Regist( DUPLICATE_EMAIL_RES, ResConfirmDuplicateInfo );
    }

    public void ActiveErrorPanel( bool _isActive )
    {
        errorPanel.SetActive( _isActive );
        if ( _isActive )
        {
            type = LoginPanelType.Error;
            password.DeactivateInputField();
            email.DeactivateInputField();
            nickname.DeactivateInputField();
        }
        else
        {
            type = LoginPanelType.Default;
            email.ActivateInputField();
        }
    }
    
    public void ActiveSignUpPanel( bool _isActive )
    {
        signUpPanel.SetActive( _isActive );
        if ( _isActive )
        {
            type = LoginPanelType.SignUp;
            nickname.ActivateInputField();
        }
        else
        {
            type = LoginPanelType.Default;
            email.ActivateInputField();
        }
    }

    private void Update()
    {
        if ( type == LoginPanelType.Default &&
                     Input.GetKeyDown( KeyCode.Tab ) )
        {
            if ( email.isFocused ) password.ActivateInputField();
            else                   email.ActivateInputField();
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            switch ( type )
            {
                case LoginPanelType.Default:
                {
                    LOGIN_INFO protocol;
                    protocol.email = email.text;
                    protocol.password = password.text;
                    protocol.nickname = string.Empty;
                    Network.Inst.Send( new Packet( CONFIRM_LOGIN_REQ, protocol ) );
                } break;

                case LoginPanelType.SignUp:
                {
                    if ( nickname.text == string.Empty )
                         break;

                    LOGIN_INFO protocol;
                    protocol.nickname = nickname.text;
                    protocol.email = email.text;
                    protocol.password = password.text;

                    Network.Inst.Send( new Packet( CONFIRM_SIGNUP_REQ, protocol ) );
                } break;

                case LoginPanelType.Error:
                {
                    ActiveErrorPanel( false );
                } break;

                case LoginPanelType.SignUpComplete:
                {
                    ActiveSignUpPanel( false );
                } break;
            } 
        }
    }

    public void ReqConfirmDuplicateInfo()
    {
        LOGIN_INFO protocol;
        protocol.email    = email.text;
        protocol.password = password.text;
        protocol.nickname = string.Empty;
        Network.Inst.Send( new Packet( DUPLICATE_EMAIL_REQ, protocol ) );
    }

    private void ResConfirmDuplicateInfo( Packet _packet )
    {
        var data = Global.FromJson<CONFIRM>( _packet );
        if ( data.isCompleted )
        {
            ActiveSignUpPanel( true );
            signUpExit.SetActive( false );
            signUpMessage.text = string.Empty;
            nickname.text      = string.Empty;
        }
        else
        {
            ActiveErrorPanel( true );
            errorMessage.text = "중복된 아이디 입니다.";
        }
    }

    public void ResAddToDatabase( Packet _packet )
    {
        var data = Global.FromJson<CONFIRM>( _packet );

        if ( data.isCompleted )
        {
            type = LoginPanelType.SignUpComplete;
            signUpExit.SetActive( true );
            signUpMessage.text = "가입 완료";
        }
        else
        {
            signUpMessage.text = "중복된 닉네임 입니다.";
        }
    }


    private void ResConfirmMatchData( Packet _packet )
    {
        var data = Global.FromJson<LOGIN_INFO>( _packet );
        if ( data.nickname == string.Empty )
        {
            ActiveErrorPanel( true );
            errorMessage.text = "아이디 또는 비밀번호가 일치하지 않습니다.";
        }
        else
            Debug.Log( $"{data.nickname} 유저가 입장했습니다." );
    }
}

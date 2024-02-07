using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginSystem : MonoBehaviour
{
    [Flags]
    enum LoginType
    {
        Default     = 0,
        LoginFail   = 1 << 0,
        AccountFail = 1 << 1,
    }
    public TMP_InputField email;
    public TMP_InputField password;

    [Header( "Panel" )]
    public GameObject loginFail;
    public GameObject AccountFail;
    LoginType type;

    private void Awake()
    {
        email?.ActivateInputField();

        if ( password != null )
             password.contentType = TMP_InputField.ContentType.Password;
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( new ResLogin(), ResponseLogin );
    }

    public void ActiveLoginFail( bool _isActive )
    {
        type = _isActive ? type |=  LoginType.LoginFail :
                           type &= ~LoginType.LoginFail;

        loginFail.SetActive( _isActive );
    }

    public void ActiveAccountFail( bool _isActive )
    {
        type = _isActive ? type |=  LoginType.AccountFail :
                           type &= ~LoginType.AccountFail;

        AccountFail.SetActive( _isActive );
    }

    private void Update()
    {
        if ( type == LoginType.Default )
        {
            if ( Input.GetKeyDown( KeyCode.Tab ) )
            {
                if ( email.isFocused )
                {
                    email.DeactivateInputField();
                    password.ActivateInputField();
                }
                else
                {
                    email.ActivateInputField();
                    password.DeactivateInputField();
                }
            }
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            if ( type == LoginType.Default )
            {
                ReqLogin protocol;
                protocol.email = email.text;
                protocol.password = password.text;
                Network.Inst.Send( new Packet( protocol ) );
            }

            if ( ( type & LoginType.LoginFail ) != 0 )
            {
                ActiveLoginFail( false );
            }


            if ( ( type & LoginType.AccountFail ) != 0 )
            {
                ActiveAccountFail( false );
            }
        }
    }

    private void ReqAccount ()
    {
        // 가입 패킷 보내기
        // ReqLogin protocol;
        // protocol.email = email.text;
        // protocol.password = password.text;
        // Network.Inst.Send( new Packet( protocol ) );
    }

    private void ResponseLogin( Packet _packet )
    {
        var data = Global.FromJson<ResLogin>( _packet );
        if ( data.nickname == string.Empty )
        {
            type |= LoginType.LoginFail;
            loginFail.SetActive( true );
        }
        else
            Debug.Log( $"{data.nickname} 유저가 입장했습니다." );
    }
}

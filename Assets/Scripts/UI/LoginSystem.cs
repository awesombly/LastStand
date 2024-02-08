using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Sprites;

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

    private void Awake()
    {
        email?.ActivateInputField();

        if ( password != null )
             password.contentType = TMP_InputField.ContentType.Password;
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( new ResLogin(),      ResponseLogin );
        ProtocolSystem.Inst.Regist( new ResSignUp(),     ResponseSignUp );
        ProtocolSystem.Inst.Regist( new ResSignUpMail(), ResponseSignUpMail );
    }

    public void ActiveErrorPanel( bool _isActive )
    {
        errorPanel.SetActive( _isActive );
    }
    
    public void ActiveSignUpPanel( bool _isActive )
    {
        signUpPanel.SetActive( _isActive );
    }

    public void ActiveInitialize()
    {
        errorPanel.SetActive( false );
        signUpPanel.SetActive( false );
        signUpExit.SetActive( false );
    }


    private void Update()
    {
        bool isDefault = !errorPanel.activeInHierarchy &&
                         !signUpPanel.activeInHierarchy;

        if ( isDefault )
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
            if ( isDefault )
            {
                ReqLogin protocol;
                protocol.email = email.text;
                protocol.password = password.text;
                Network.Inst.Send( new Packet( protocol ) );
            }

            if ( errorPanel.activeInHierarchy )
            {
                ActiveErrorPanel( false );
            }

            if ( signUpPanel.activeInHierarchy && nickname.text != string.Empty )
            {
                ReqSignUp protocol;
                protocol.nickname = nickname.text;
                protocol.email    = email.text;
                protocol.password = password.text;

                Network.Inst.Send( new Packet( protocol ) );
            }
        }
    }

    public void RequestSignUpMail()
    {
        ReqSignUpMail protocol;
        protocol.email    = email.text;
        protocol.password = password.text;
        Network.Inst.Send( new Packet( protocol ) );
    }

    private void ResponseSignUpMail( Packet _packet )
    {
        var data = Global.FromJson<ResSignUpMail>( _packet );
        if ( data.isPossible )
        {
            signUpPanel.SetActive( true );
            signUpMessage.text = string.Empty;
        }
        else
        {
            ActiveErrorPanel( true );
            errorMessage.text = "중복된 아이디 입니다.";
        }
    }

    public void ResponseSignUp( Packet _packet )
    {
        var data = Global.FromJson<ResSignUp>( _packet );

        if ( data.isCompleted )
        {
            signUpExit.SetActive( true ); 
            signUpMessage.text = "가입 완료";
        }
        else
        {
            signUpMessage.text = "중복된 닉네임 입니다.";
        }
    }


    private void ResponseLogin( Packet _packet )
    {
        var data = Global.FromJson<ResLogin>( _packet );
        if ( data.nickname == string.Empty )
        {
            ActiveErrorPanel( true );
            errorMessage.text = "아이디 또는 비밀번호가 일치하지 않습니다.";
        }
        else
            Debug.Log( $"{data.nickname} 유저가 입장했습니다." );
    }
}

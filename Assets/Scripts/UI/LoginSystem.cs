using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginSystem : MonoBehaviour
{
    public TMP_InputField email;
    public TMP_InputField password;

    private void Awake()
    {
        if ( password != null )
             password.contentType = TMP_InputField.ContentType.Password;
    }

    private void Start()
    {
        ProtocolSystem.Inst.Regist( new ResLogin(), ResponseLogin );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            ReqLogin protocol;
            protocol.email    = email.text;
            protocol.password = password.text;
            Network.Inst.Send( new Packet( protocol ) );
        }
    }

    private void ResponseLogin( Packet _packet )
    {
        var data = Global.FromJson<ResLogin>( _packet );
        if ( data.nickname == string.Empty )
            Debug.Log( "�̸��� �Ǵ� ��й�ȣ�� Ȯ�����ּ���." );
        else
            Debug.Log( $"{data.nickname} ������ �����߽��ϴ�." );
    }
}

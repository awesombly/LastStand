using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PacketType;

public class Sample : MonoBehaviour
{
    public GameObject pause;
    private bool isProgress;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK, AckExitStage );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if ( pause.activeInHierarchy ) pause.SetActive( false );
            else                           pause.SetActive( true );
        }
    }

    public void ExitStageReq()
    {
        if ( isProgress )
             return;

        isProgress = true;
        Network.Inst.Send( EXIT_STAGE_REQ, new EMPTY() );
    }


    private void AckExitStage( Packet _packet )
    {
        isProgress = false;
        StageSystem.Info = null;
        SceneBase.ChangeScene( SceneType.Lobby );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PacketType;

public class Sample : MonoBehaviour
{
    private void Awake()
    {
        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK, AckExitStage );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Escape ) )
             Network.Inst.Send( EXIT_STAGE_REQ, new EMPTY() );
    }

    private void AckExitStage( Packet _packet )
    {
        StageSystem.Info = null;
        SceneBase.ChangeScene( SceneType.Lobby );
    }
}

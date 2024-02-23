using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;

public class InGameUIScene : SceneBase
{
    public Camera uiCamera;

    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    public TextMeshProUGUI targetKillCount;

    public GameObject pause;
    private bool isProgress;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;

        ProtocolSystem.Inst.Regist( EXIT_STAGE_ACK, AckExitStage );

        if ( !ReferenceEquals( StageSystem.StageInfo, null ) ) 
             targetKillCount.text = $"{StageSystem.StageInfo.Value.targetKill}";
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if ( pause.activeInHierarchy ) pause.SetActive( false );
            else pause.SetActive( true );
        }
    }

    public void ReqExitStage()
    {
        if ( isProgress || StageSystem.StageInfo == null )
            return;

        isProgress = true;
        Network.Inst.Send( EXIT_STAGE_REQ, StageSystem.StageInfo.Value );
    }

    private void AckExitStage( Packet _packet )
    {
        isProgress = false;
        StageSystem.StageInfo = null;
        SceneBase.LoadScene( SceneType.Lobby );
    }
}

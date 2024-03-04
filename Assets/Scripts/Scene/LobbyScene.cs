using System.Collections;
using UnityEngine;

using static PacketType;

public class LobbyScene : SceneBase
{
    public Camera uiCamera;
    public Transform cursor;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Lobby;

        var net = Network.Inst;
        StartCoroutine( WaitForAudioLoad() );
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
    #endregion
    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGM.Lobby, 0f, .5f, 5f );
    }
}

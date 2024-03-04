using System.Collections;
using UnityEngine;

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

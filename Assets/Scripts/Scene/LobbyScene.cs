using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class LobbyScene : SceneBase
{
    [Header( "< Default >" )]
    public Camera uiCamera;
    public Transform cursor;

    [Header( "< Error Panel >" )]
    [SerializeField] GameObject      errorPanel;
    [SerializeField] TextMeshProUGUI errorText;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Lobby;
        StartCoroutine( WaitForAudioLoad() );

        OnDisconnected();
        Network.Inst.OnDisconnected += OnDisconnected;

        #if !UNITY_EDITOR
        Cursor.visible = false;
        #endif
    }

    private void Update()
    {
        Vector3 pos = Input.mousePosition;
        cursor.position = uiCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, 10f ) );
    }

    private void OnDestroy()
    {
        Network.Inst.OnDisconnected -= OnDisconnected;
    }
    #endregion

    private void OnDisconnected()
    {
        if ( !Network.Inst.IsConnected )
             Network.Inst.Connect( "127.0.0.1" );
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGM.Lobby, 0f, .5f, 5f );
    }

    public void ExitGame() => Application.Quit();
    
    public void ActiveErrorPanel( string _text )
    {
        errorPanel.SetActive( true );
        errorText.text = _text;
    }
}

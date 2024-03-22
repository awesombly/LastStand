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
        
        if ( !Network.Inst.IsConnected )
             Network.Inst.SelectIP( "127.0.0.1" );

        #if !UNITY_EDITOR
        Cursor.visible = false;
        #endif
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

    public void ExitGame() => Application.Quit();
    
    public void ActiveErrorPanel( string _text )
    {
        errorPanel.SetActive( true );
        errorText.text = _text;
    }
}

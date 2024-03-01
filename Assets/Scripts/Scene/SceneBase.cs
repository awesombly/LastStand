using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;

[System.Serializable]
public enum SceneType
{
    None,
    Lobby,
    InGame_Logic,
    InGame_UI,
}

[RequireComponent( typeof( SpriteRenderer ) )]
public class SceneBase : MonoBehaviour
{
    public static event Action OnGameStart;
    public static event Action OnBeforeSceneLoad;
    public static event Action OnAfterSceneLoad;

    private SpriteRenderer fadeSprite;
    private static readonly float FadeTime = .65f;
    private static readonly float FadeWaitTime = .025f;
    private static readonly float FadeDuration = FadeTime + ( FadeWaitTime * 2f );

    public SceneType SceneType { get; protected set; }
    public bool IsSending { get; protected set; }

    // 게임 시작시 자동 실행
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
    private static void BeforeGameStart()
    {
        DOTween.Init( true, false, LogBehaviour.Default ).SetCapacity( 50, 20 );
        OnGameStart?.Invoke();
    }

    public void LoadScene( SceneType _sceneType, LoadSceneMode _loadMode = LoadSceneMode.Single )
    {
        FadeOut( () =>
        {
            OnBeforeSceneLoad?.Invoke();
            SceneManager.LoadScene( _sceneType.ToString(), _loadMode );
        } );
    }

    protected virtual void Awake()
    {
        SceneType = SceneType.None;
        CreateFadeSprite();
        EnabledInputSystem( true, true );
    }

    protected virtual void Start()
    {
        FadeIn();
    }

    protected virtual void OnEnable()
    {
        OnAfterSceneLoad?.Invoke();
    }


    #region Input
    protected void EnabledInputSystem( bool _keyboard, bool _mouse )
    {
        if( _keyboard ) InputSystem.EnableDevice(  Keyboard.current );
        else            InputSystem.DisableDevice( Keyboard.current );

        if ( _mouse ) InputSystem.EnableDevice(  Mouse.current );
        else          InputSystem.DisableDevice( Mouse.current );
    }
    #endregion

    #region Effect
    private void CreateFadeSprite()
    {
        if ( TryGetComponent( out fadeSprite ) )
        {
            Texture2D tex           = Texture2D.whiteTexture;
            gameObject.layer        = Global.Layer.UI;
            fadeSprite.sprite       = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
            fadeSprite.drawMode     = SpriteDrawMode.Sliced;
            fadeSprite.size         = new Vector2( 10000f, 10000f );
            fadeSprite.sortingOrder = 1000;
            transform.position      = new Vector3( 0f, 0f, 10f );
            transform.localScale    = Vector3.one;
        }
    }

    private void FadeIn( Action _onCompleted = null )  => StartCoroutine( SceneFadeIn( _onCompleted ) );

    private void FadeOut( Action _onCompleted = null ) => StartCoroutine( SceneFadeOut( _onCompleted ) );

    private IEnumerator SceneFadeIn( Action _onCompleted = null )
    {
        EnabledInputSystem( false, false );
        fadeSprite.color = Color.black;
        fadeSprite.enabled = true;

        fadeSprite.DOFade( 0f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeDuration );
        fadeSprite.color = Color.clear;
        EnabledInputSystem( true, true );
        fadeSprite.enabled = false;
        _onCompleted?.Invoke();
    }

    private IEnumerator SceneFadeOut( Action _onCompleted = null )
    {
        EnabledInputSystem( false, false );
        fadeSprite.color = Color.clear;
        fadeSprite.enabled = true;

        fadeSprite.DOFade( 1f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeDuration );
        fadeSprite.color = Color.black;
        _onCompleted?.Invoke();
    }
    #endregion



    #region Sound
    public void OnMasterVolumeChanged( float _volume )
    {
        AudioManager.Inst.MixerDecibelControl( MixerType.Master, _volume );
    }

    public void OnBGMVolumeChanged( float _volume )
    {
        AudioManager.Inst.MixerDecibelControl( MixerType.BGM, _volume );
    }

    public void OnSFXVolumeChanged( float _volume )
    {
        AudioManager.Inst.MixerDecibelControl( MixerType.SFX, _volume );
    }
    
    public void MenuSelectSound()
    {
        AudioManager.Inst.Play( SFX.MouseClick );
    }
    #endregion
}

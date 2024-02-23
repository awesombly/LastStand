using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum SceneType
{
    None,
    Lobby,
    InGame_Logic,
    InGame_UI,
}

public class SceneBase : MonoBehaviour
{
    public static event Action OnGameStart;
    public static event Action OnBeforeSceneLoad;
    public static event Action OnAfterSceneLoad;

    public SceneType SceneType { get; protected set; }

    // ���� ���۽� �ڵ� ����
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
    private static void BeforeGameStart()
    {
        OnGameStart?.Invoke();
    }

    public static void LoadScene( SceneType _sceneType, LoadSceneMode _loadMode = LoadSceneMode.Single )
    {
        OnBeforeSceneLoad?.Invoke();
        SceneManager.LoadScene( _sceneType.ToString(), _loadMode );
    }

    protected virtual void Awake()
    {
        SceneType = SceneType.None;
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnEnable()
    {
        OnAfterSceneLoad?.Invoke();
    }

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

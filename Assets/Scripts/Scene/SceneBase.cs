using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum SceneType
{
    None,
    Login,
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

    // 게임 시작시 자동 실행
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
}

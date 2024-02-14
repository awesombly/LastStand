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
    InGame,
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

    public static void ChangeScene( SceneType _sceneType )
    {
        OnBeforeSceneLoad?.Invoke();
        SceneManager.LoadScene( _sceneType.ToString() );
    }

    protected virtual void Awake()
    {
        SceneType = SceneType.None;
    }

    protected virtual void Start()
    {
        OnAfterSceneLoad?.Invoke();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum SceneType
{
    Login,
    Lobby,
    InGame,
}

public class SceneBase : MonoBehaviour
{
    public static event Action OnGameStart;
    public static event Action OnBeforeSceneLoad;
    public static event Action OnAfterSceneLoad;


    // 게임 시작시 자동 실행
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

    protected virtual void Start()
    {
        OnAfterSceneLoad?.Invoke();
    }
}

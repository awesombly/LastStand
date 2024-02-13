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
    public static void ChangeScene( SceneType _sceneType )
    {
        SceneManager.LoadScene( _sceneType.ToString() );
    }
}

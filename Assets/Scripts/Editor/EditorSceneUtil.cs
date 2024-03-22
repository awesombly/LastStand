using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class EditorSceneUtil : MonoBehaviour
{
    [MenuItem( "MyEditor/CurrentScene Start" )]
    public static void StartCurrentScene()
    {
        EditorSceneManager.playModeStartScene = null;
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [MenuItem( "MyEditor/InitScene Start" )]
    public static void SetupInitScene()
    {
        StartScene( SceneType.Init );
    }

    [MenuItem( "MyEditor/LobbyScene Start" )]
    public static void SetupLobbyScene()
    {
        StartScene( SceneType.Lobby );
    }

    private static void StartScene( SceneType _sceneType )
    {
        string scenePath = null;
        foreach ( var scene in EditorBuildSettings.scenes )
        {
            string sceneName = _sceneType.ToString();
            if ( scene.path.Contains( sceneName ) )
            {
                scenePath = scene.path;
                break;
            }
        }

        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>( scenePath );
        EditorSceneManager.playModeStartScene = sceneAsset;
        UnityEditor.EditorApplication.isPlaying = true;
    }
}

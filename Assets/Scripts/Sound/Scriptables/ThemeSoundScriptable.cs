using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThemeType : byte
{
    Default,
    Theme_1,
    Theme_2,
}

public enum ThemeSound : byte
{
    Login,
    Lobby,
    InGame,

    MouseClick,
    MouseHover,

    MenuEntry,
    MenuExit,
}

[CreateAssetMenu( fileName = "ThemeSound", menuName = "Scriptable Objects/ThemeSound" )]
public class ThemeSoundScriptable : ScriptableObject
{
    [System.Serializable]
    public struct ThemeData
    {
        public ThemeSound soundType;
        public AudioClip clip;
    }

    public ThemeType type;
    [SerializeField]
    public List<ThemeData> datas = new List<ThemeData>();
}
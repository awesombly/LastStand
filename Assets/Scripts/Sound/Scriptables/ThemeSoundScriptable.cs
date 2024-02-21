using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[CreateAssetMenu( fileName = "ThemeSound", menuName = "Scriptable Objects/ThemeSound" )]
public class ThemeSoundScriptable : ScriptableObject
{
    public enum ThemeType : byte
    {
        Default,
        Interface_1,
        Interface_2,
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

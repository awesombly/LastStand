using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThemeType : ushort
{
    Default,
    Theme_1,
    Theme_2,
}

public enum ThemeSound : ushort
{
    Login,
    Lobby,
    InGame,

    MouseClick,
    MouseHover,

    MenuEntry,
    MenuExit,
}

[CreateAssetMenu( fileName = "AudioData_Theme", menuName = "Scriptable Objects/AudioData_Theme" )]
public class AudioDataTheme : ScriptableObject
{
    public AudioClip Login;
    public AudioClip Lobby;
    public AudioClip InGame;

    public AudioClip MouseClick;
    public AudioClip MouseHover;
    public AudioClip MenuEntry;
    public AudioClip MenuExit;
}
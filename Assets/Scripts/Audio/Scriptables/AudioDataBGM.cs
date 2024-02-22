using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BGMType : ushort
{
    Default,
    Theme_1, 
    Theme_2, 
}

public enum BGMSound : ushort
{
    Login,
    Lobby,
    InGame,
}

[CreateAssetMenu( fileName = "AudioData_BGM", menuName = "Scriptable Objects/AudioData_BGM" )]
public class AudioDataBGM : ScriptableObject
{
    public AudioClip Login;
    public AudioClip Lobby;
    public AudioClip InGame;
}
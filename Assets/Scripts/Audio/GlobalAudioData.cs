using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu( fileName = "AudioData", menuName = "Scriptable Objects/GlobalAudioData" )]
public class GlobalAudioData : ScriptableObject
{
    public AudioMixer mixer;
    public AudioChannel channel;

    [Header( "BGM" )]
    public AudioClip bgmLobby;
    public AudioClip bgmInGame;

    [Header( "SFX" )]
    public AudioClip sfxMouseClick;
    public AudioClip sfxMenuEntry;
    public AudioClip sfxMenuExit;
}

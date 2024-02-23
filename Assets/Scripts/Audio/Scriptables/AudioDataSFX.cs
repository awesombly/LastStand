using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType : ushort
{
    Default,
    Theme_1,
    Theme_2,
}

public enum SFXSound : ushort
{
    MouseClick,

    MenuEntry,
    MenuExit,
}

[CreateAssetMenu( fileName = "AudioData_SFX", menuName = "Scriptable Objects/AudioData_SFX" )]
public class AudioDataSFX : ScriptableObject
{
    public AudioClip MouseClick;
    public AudioClip MenuEntry;
    public AudioClip MenuExit;
}
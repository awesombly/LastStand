using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType : ushort
{
    Default,
    Player_1, // 캐릭터 1
    Player_2, // 캐릭터 2
}

public enum PlayerSound : ushort
{
    Attack,
    Hit,
    Dead,
}

[CreateAssetMenu( fileName = "AudioData_Player", menuName = "Scriptable Objects/AudioData_Player" )]
public class AudioDataPlayer : ScriptableObject
{
    public AudioClip Attack;
    public AudioClip Hit;
    public AudioClip Dead;
}
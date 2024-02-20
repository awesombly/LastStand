using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[CreateAssetMenu( fileName = "SoundData", menuName = "Scriptable Objects/SoundData" )]
public class SoundData : ScriptableObject
{
    [SerializeField]
    public List<Sound> sounds = new List<Sound>();
}
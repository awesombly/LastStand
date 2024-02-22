using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType : byte
{
    Default,
    Player_1, // 캐릭터 1
    Player_2, // 캐릭터 2
}

public enum PlayerSound : byte
{
    Attack,
    Hit,
    Dead,
}

[CreateAssetMenu( fileName = "PlayerSound", menuName = "Scriptable Objects/PlayerSound" )]
public class PlayerSoundScriptable : ScriptableObject
{
    [System.Serializable]
    public struct PlayerSoundData
    {
        public PlayerSound soundType;
        public AudioClip clip;
    }

    public PlayerType type;
    [SerializeField]
    public List<PlayerSoundData> datas = new List<PlayerSoundData>();
}
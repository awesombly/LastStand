using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[CreateAssetMenu( fileName = "PlayerSound", menuName = "Scriptable Objects/PlayerSound" )]
public class PlayerSoundScriptable : ScriptableObject
{
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
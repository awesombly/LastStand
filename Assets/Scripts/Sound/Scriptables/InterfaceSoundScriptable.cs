using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[CreateAssetMenu( fileName = "InterfaceSound", menuName = "Scriptable Objects/InterfaceSound" )]
public class InterfaceSoundScriptable : ScriptableObject
{
    public enum InterfaceType : byte
    {
        Default,
        Interface_1,
        Interface_2,
    }

    public enum InterfaceSound : byte
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
    public struct InterfaceData
    {
        public InterfaceSound soundType;
        public AudioClip clip;
    }

    public InterfaceType type;
    [SerializeField]
    public List<InterfaceData> datas = new List<InterfaceData>();
}

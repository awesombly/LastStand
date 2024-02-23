using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType : ushort
{
    Default,
    Handgun,
    Rifle,
    BurstRifle,
    Shotgun,
    AssaultShotgun,
}

public enum WeaponSound : ushort
{
    Fire,
    Hit,
    Reload,
    Swap,
}

[CreateAssetMenu( fileName = "AudioData_Weapon", menuName = "Scriptable Objects/AudioData_Weapon" )]
public class AudioDataWeapon : ScriptableObject
{
    public AudioClip Fire;
    public AudioClip Hit;
    public AudioClip Reload;
    public AudioClip Swap;
}

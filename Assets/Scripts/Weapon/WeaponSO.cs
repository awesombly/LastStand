using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "WeaponSO_", menuName = "Scriptable Objects/WeaponSO" )]
public class WeaponSO : ScriptableObject
{
    [Header( "¦¡ Sound" )]
    public AudioClip Fire;
    public AudioClip Hit;
    public AudioClip Reload;
    public AudioClip Swap;
}

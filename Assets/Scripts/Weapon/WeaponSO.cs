using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "WeaponSO_", menuName = "Scriptable Objects/WeaponSO" )]
public class WeaponSO : ScriptableObject
{
    [Header( "¦¡ Sound" )]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip swapSound;
}

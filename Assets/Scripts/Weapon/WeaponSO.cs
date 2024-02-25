using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "WeaponSO_", menuName = "Scriptable Objects/WeaponSO" )]
public class WeaponSO : ScriptableObject
{
    [SerializeField]
    public Bullet bulletPrefab;

    [Serializable]
    public struct StatInfo
    {
        [Min( 0 )]
        public int ammo;
        [Min( 0 )]
        public int magazine;
        [Min( 0f )]
        public float repeatDelay;
        [Min( 0.001f )]
        public float reloadDelay;
        [Min( 0f )]
        public float swapDelay;
        public bool isAllowKeyHold;
        [Min( 0f )]
        public float shakeShotAngle;
    }
    [Header( "¦¡ Stat" )]
    public StatInfo stat;

    [Serializable]
    public struct MultiShotInfo
    {
        [Min( 1 )]
        public int bulletPerShot;
        [Min( 0f )]
        public float spreadAngle;
        [Range( 0f, 1f )]
        public float speedRate;
        [Min( 1 )]
        public int burstCount;
        [Min( 0f )]
        public float burstDelay;
    }
    [Header( "¦¡ Extend" )]
    public MultiShotInfo shotInfo;

    [Header( "¦¡ Sound" )]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip swapSound;
}

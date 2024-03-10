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
        [Tooltip( "총 탄약" ), Min( 0 )]
        public int ammo;
        [Tooltip( "탄창 크기" ), Min( 0 )]
        public int magazine;
        [Tooltip( "사격간 딜레이" ), Min( 0f )]
        public float repeatDelay;
        [Tooltip( "재장전 딜레이" ), Min( 0.001f )]
        public float reloadDelay;
        [Tooltip( "해당 무기로 교체시 딜레이" ), Min( 0f )]
        public float swapDelay;
        [Tooltip( "발사키 유지시 자동 사격 여부" )]
        public bool isAllowKeyHold;
        [Tooltip( "발사각 흔들림" ), Min( 0f )]
        public float shakeShotAngle;
        [Tooltip( "발사시 사격자가 밀려나는 힘" ), Min( 0f )]
        public float reactionPower;
    }
    [Header( "─ Stat" )]
    public StatInfo stat;

    [Serializable]
    public struct MultiShotInfo
    {
        [Tooltip( "한 번 발사할 때 탄 갯수" ), Min( 1 )]
        public int bulletPerShot;
        [Tooltip( "한 번 발사할 때 발사각 흔들림" ), Min( 0f )]
        public float spreadAngle;
        [Tooltip( "발사시 탄속 랜덤화" ), Range( 0f, 1f )]
        public float speedRate;
        [Tooltip( "점사 횟수" ), Min( 1 )]
        public int burstCount;
        [Tooltip( "점사 딜레이" ), Min( 0f )]
        public float burstDelay;
    }
    [Header( "─ Extend" )]
    public MultiShotInfo shotInfo;

    [Header( "─ Effect" )]
    public Poolable fireEffect;
    public Poolable reloadEffect;

    [Header( "─ Sound" )]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip swapSound;
}

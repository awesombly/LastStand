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
        [Tooltip( "�� ź��" ), Min( 0 )]
        public int ammo;
        [Tooltip( "źâ ũ��" ), Min( 0 )]
        public int magazine;
        [Tooltip( "��ݰ� ������" ), Min( 0f )]
        public float repeatDelay;
        [Tooltip( "������ ������" ), Min( 0.001f )]
        public float reloadDelay;
        [Tooltip( "�ش� ����� ��ü�� ������" ), Min( 0f )]
        public float swapDelay;
        [Tooltip( "�߻�Ű ������ �ڵ� ��� ����" )]
        public bool isAllowKeyHold;
        [Tooltip( "�߻簢 ��鸲" ), Min( 0f )]
        public float shakeShotAngle;
        [Tooltip( "�߻�� ����ڰ� �з����� ��" ), Min( 0f )]
        public float reactionPower;
    }
    [Header( "�� Stat" )]
    public StatInfo stat;

    [Serializable]
    public struct MultiShotInfo
    {
        [Tooltip( "�� �� �߻��� �� ź ����" ), Min( 1 )]
        public int bulletPerShot;
        [Tooltip( "�� �� �߻��� �� �߻簢 ��鸲" ), Min( 0f )]
        public float spreadAngle;
        [Tooltip( "�߻�� ź�� ����ȭ" ), Range( 0f, 1f )]
        public float speedRate;
        [Tooltip( "���� Ƚ��" ), Min( 1 )]
        public int burstCount;
        [Tooltip( "���� ������" ), Min( 0f )]
        public float burstDelay;
    }
    [Header( "�� Extend" )]
    public MultiShotInfo shotInfo;

    [Header( "�� Effect" )]
    public Poolable fireEffect;
    public Poolable reloadEffect;

    [Header( "�� Sound" )]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip swapSound;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "BulletSO_", menuName = "Scriptable Objects/BulletSO" )]
public class BulletSO : ScriptableObject
{
    [Header( "¦¡ Stat" )]
    public float moveSpeed;
    public float damage;
    [Min( 0f )]
    public float range;
    public float pushingPower;
    [Min( 1f )]
    public int penetratePower;

    [Header( "¦¡ Effect" )]
    public Poolable fireEffect;
    public Poolable hitEffect;

    [Header( "¦¡ Sound" )]
    public AudioClip hitSound;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "BulletSO_", menuName = "Scriptable Objects/BulletSO" )]
public class BulletSO : ScriptableObject
{
    public float moveSpeed;
    public float damage;
    public float range;
    public float pushingPower;
    public Global.StatusInt penetratePower;

    public Poolable fireEffect;
    public Poolable hitEffect;

    public PlayerSoundScriptable.PlayerType playerType;
    [Range( 0f, 1f )]
    public float volume;
}

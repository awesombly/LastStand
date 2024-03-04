using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : EffectSpawner
{
    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnHitEvent += OnHit;
        bullet.OnFireEvent += OnFire;
    }

    private void OnFire( Bullet _bullet )
    {
        SpawnEffect( _bullet.data.fireEffect, _bullet.transform );
    }

    private void OnHit( Bullet _bullet )
    {
        SpawnEffect( _bullet.data.hitEffect, _bullet.transform );
    }
}

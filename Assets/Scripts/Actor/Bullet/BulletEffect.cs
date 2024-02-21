using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnHit += OnHit;
        bullet.OnFire += OnFire;
    }

    private void SpawnEffect( Poolable _effect, Transform _transform )
    {
        if ( ReferenceEquals( _effect, null ) )
        {
            return;
        }

        Poolable poolable = PoolManager.Inst.Get( _effect );
        var particle = poolable.GetComponent<ParticleSystem>();
        if ( ReferenceEquals( particle, null ) )
        {
            return;
        }
        particle.transform.SetPositionAndRotation( _transform.position, _transform.rotation );
        particle.transform.rotation = _transform.rotation;
        particle.Play( true );
    }

    private void OnFire( Bullet _bullet )
    {
        SpawnEffect( _bullet.data.fireEffect, _bullet.transform );
    }

    private void OnHit( Character _attacker, Character _defender, Bullet _bullet )
    {
        SpawnEffect( _bullet.data.hitEffect, _bullet.transform );
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawner : MonoBehaviour 
{
    protected virtual void SpawnEffect( Poolable _effect, Transform _transform )
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
        particle.Play( true );
    }
}

public class PlayerEffect : EffectSpawner
{
    private Player player;

    void Start()
    {
        player = GetComponent<Player>();

        var movement = GetComponent<PlayerMovement>();
        movement.OnDodgeAction += OnDodgeAction;

        player.DodgeAttack.OnHitEvent += OnDodgeHit;
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction, float _duration )
    {
        if ( _isActive )
        {
            SpawnEffect( player.playerData.dodgeActionEffect, player.transform );
        }
    }

    private void OnDodgeHit( Player _attacker, Actor _defender )
    {
        SpawnEffect( player.playerData.dodgeHitEffect, _defender.transform );
    }
}

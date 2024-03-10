using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawner : MonoBehaviour 
{
    protected virtual void SpawnEffect( Poolable _prefab, Transform _transform )
    {
        if ( _prefab == null )
        {
            return;
        }

        Poolable poolable = PoolManager.Inst.Get( _prefab );
        var particle = poolable.GetComponent<ParticleSystem>();
        if ( particle is null )
        {
            return;
        }
        particle.transform.SetPositionAndRotation( _transform.position, _transform.rotation );
        particle.transform.Rotate( _prefab.transform.rotation.eulerAngles );
        particle.Play( true );
    }
}

public class PlayerEffect : EffectSpawner
{
    private Sprite curDodgeSprite = null;

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();

        var movement = GetComponent<PlayerMovement>();
        movement.OnDodgeAction += OnDodgeAction;
        movement.dodgeInfo.duration.OnChangeCurrent += OnChangeDodgeDuration;

        player.DodgeAttack.OnHitEvent += OnDodgeHit;
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction, float _duration )
    {
        if ( _isActive )
        {
            SpawnEffect( player.playerData.dodge.actionEffect, player.transform );
        }
    }

    private void OnDodgeHit( Player _attacker, Actor _defender )
    {
        SpawnEffect( player.playerData.dodge.hitEffect, _defender.transform );
    }

    private void OnChangeDodgeDuration( float _old, float _new, float _max )
    {
        if ( !ReferenceEquals( curDodgeSprite, player.Spriter.sprite ) )
        {
            curDodgeSprite = player.Spriter.sprite;
            Poolable afterImage = PoolManager.Inst.Get( player.playerData.dodge.afeterImage );
            afterImage.transform.position = transform.position;

            Color originColor = afterImage.Spriter.color;
            afterImage.Spriter.sprite = curDodgeSprite;
            afterImage.Spriter.DOFade( 0.2f, player.playerData.dodge.afterImageDuration )
                .onComplete = () => 
                {
                    afterImage.Spriter.color = originColor;
                    afterImage.Release(); 
                };
        }
    }
}

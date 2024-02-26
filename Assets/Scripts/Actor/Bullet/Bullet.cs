using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Actor
{
    public BulletSO data;
    private float lifeTime;
    private float totalDamage;

    private Character owner;

    public event Action<Bullet> OnFire;
    public event Action<Character/*attacker*/, Character/*defender*/, Bullet> OnHit;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if( !IsLocal )
        {
            return;
        }

        lifeTime -= Time.deltaTime;
        if ( lifeTime <= 0)
        {
            SERIAL_INFO protocol;
            protocol.serial = Serial;
            Network.Inst.Send( PacketType.REMOVE_ACTOR_REQ, protocol );

            Release();
        }
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        if ( !gameObject.activeSelf )
        {
            return;
        }

        Character defender = _other.GetComponent<Character>();
        if ( defender == null || owner == null )
        {
            Debug.LogWarning( $"Character is null. other:{_other.name}, owner:{owner}" );
            return;
        }

        if ( defender.IsDead )
        {
            return;
        }

        --data.penetratePower.Current;
        HitTarget( owner, defender );

        HIT_INFO protocol;
        protocol.needRelease = data.penetratePower.IsZero;
        protocol.bullet = Serial;
        protocol.attacker = owner.Serial;
        protocol.defender = defender.Serial;
        protocol.hp = defender.Hp.Current;
        Network.Inst.Send( PacketType.HIT_ACTOR_REQ, protocol );

        if ( data.penetratePower.IsZero )
        {
            Release();
        }
    }
    #endregion

    public void Fire( BULLET_SHOT_INFO _shotInfo, BULLET_INFO _bulletInfo )
    {
        IsLocal = _shotInfo.isLocal;
        Serial = _bulletInfo.serial;
        owner = GameManager.Inst.GetActor( _shotInfo.owner ) as Character;
        if ( IsLocal )
        {
            gameObject.layer = Global.Layer.PlayerAttack;
            Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Enemy | Global.LayerFlag.Misc );
        }
        else
        {
            gameObject.layer = Global.Layer.EnemyAttack;
            Rigid2D.excludeLayers = ~0;
        }
        totalDamage = _shotInfo.damage;
        lifeTime = data.range / data.moveSpeed;
        data.penetratePower.SetMax();

        transform.SetPositionAndRotation( _shotInfo.pos.To(), Quaternion.Euler( 0, 0, _bulletInfo.angle - 90 ) );
        Rigid2D.velocity = transform.up * ( data.moveSpeed * _bulletInfo.rate );

        OnFire?.Invoke( this );
    }

    public void HitTarget( Character _attacker, Character _defender )
    {
        _defender?.OnHit( _attacker, this );
        OnHit?.Invoke( _attacker, _defender, this );
    }

    public float GetDamage()
    {
        return totalDamage;
    }
}

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

    public event Action<Bullet> OnFireEvent;
    public event Action<Bullet> OnHitEvent;

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
        if ( !IsLocal || !_other.gameObject.activeSelf || ReferenceEquals( owner, null ) )
        {
            return;
        }

        Actor defender = _other.GetComponent<Actor>();
        if ( defender == null )
        {
            Debug.LogWarning( $"Actor is null. other:{_other.name}, owner:{owner}" );
            return;
        }

        HitTarget( defender );
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

        OnFireEvent?.Invoke( this );
    }

    public void HitTarget( Actor _defender )
    {
        OnHitEvent?.Invoke( this );
        _defender?.OnHit( owner, this );

        --data.penetratePower.Current;
        if ( IsLocal )
        {
            HIT_INFO protocol;
            protocol.needRelease = data.penetratePower.IsZero;
            protocol.bullet = Serial;
            protocol.attacker = owner.Serial;
            protocol.defender = _defender.Serial;
            protocol.hp = _defender.Hp.Current;
            Network.Inst.Send( PacketType.HIT_ACTOR_REQ, protocol );
        }

        if ( data.penetratePower.IsZero )
        {
            Release();
        }
    }

    public float GetDamage()
    {
        return totalDamage;
    }

    public override void OnHit( Actor _attacker, Bullet _bullet )
    {
        base.OnHit( _attacker, _bullet );
    }

    protected override void OnDead( Actor _attacker, Bullet _bullet )
    {
        base.OnDead( _attacker, _bullet );
    }
}

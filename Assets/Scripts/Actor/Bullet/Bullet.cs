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
    private Global.StatusInt penetrateCount;
    private List<uint/*Serial*/> alreadyHitActors = new List<uint>();

    private Character owner;

    public event Action<Bullet> OnFireEvent;
    public event Action<Bullet> OnHitEvent;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        penetrateCount.Max = data.penetratePower;
        Hp.Max = penetrateCount.Max * 10f;  // Bullet끼리 충돌했을 때 사용
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
            GameManager.Inst.PushRemoveActorToSend( Serial );

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

        if ( alreadyHitActors.Contains( defender.Serial ) )
        {
            return;
        }

        Bullet bullet = defender as Bullet;
        if ( !ReferenceEquals( bullet, null ) )
        {
            // Bullet끼리 충돌했을 때, 각자 클라가 처리시
            // 중복처리 및 동기화 이슈가 생길 수 있어 Serial이 높은 쪽에서 Hit시킴
            if ( bullet.Serial > Serial )
            {
                return;
            }
            bullet.HitTarget( this );
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
            Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Enemy | Global.LayerFlag.EnemyAttack | Global.LayerFlag.Misc );
        }
        else
        {
            gameObject.layer = Global.Layer.EnemyAttack;
            Rigid2D.excludeLayers = ~( int )Global.LayerFlag.PlayerAttack;
        }
        totalDamage = _shotInfo.damage;
        lifeTime = data.range / data.moveSpeed;
        penetrateCount.SetMax();
        Hp.SetMax();
        alreadyHitActors.Clear();

        transform.SetPositionAndRotation( _shotInfo.pos.To(), Quaternion.Euler( 0, 0, _bulletInfo.angle - 90 ) );
        Rigid2D.velocity = transform.up * ( data.moveSpeed * _bulletInfo.rate );

        OnFireEvent?.Invoke( this );
    }

    public void HitTarget( Actor _defender )
    {
        if ( !( _defender is Bullet ) )
        {
            --penetrateCount.Current;
        }

        OnHitEvent?.Invoke( this );
        _defender?.OnHit( owner, this );

        if ( IsLocal )
        {
            alreadyHitActors.Add( _defender.Serial );

            HIT_INFO hit;
            hit.needRelease = penetrateCount.IsZero;
            hit.bullet = Serial;
            hit.attacker = owner.Serial;
            hit.defender = _defender.Serial;
            hit.pos = new VECTOR2( transform.position );
            hit.hp = _defender.Hp.Current;
            GameManager.Inst.PushHitInfoToSend( hit );
        }

        if ( penetrateCount.IsZero )
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
        Release();
    }
}

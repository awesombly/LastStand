using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SyncType
{
    LocalFirst,     // ���� Ŭ�󿡼� ����ó��
    LocalEcho,      // ������ ���ļ�, ���� Ŭ�� �ٽ� �޾Ƽ� ó��
    FromServer,     // �ٸ� Ŭ�󿡼� ���� ����ȭ ó��
}

public interface IHitable
{
    public void HitTarget( Actor _defender, SyncType _syncType , float _serverHp = 0f );
    public float GetDamage();
    public Vector2 GetUpDirection();
    public Vector2 GetPushingForce();
}

public class Bullet : Actor, IHitable
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
        Hp.Max = GetInitHp();  // Bullet���� �浹���� �� ���
    }

    private void Update()
    {
        if( !IsLocal || !Rigid2D.simulated )
        {
            return;
        }

        lifeTime -= Time.deltaTime;
        if ( lifeTime <= 0)
        {
            GameManager.Inst.PushRemoveActorToSend( Serial );
            // ���� Release�� ���� ������ �ް� �Ѵ�
            Rigid2D.simulated = false;
        }
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        if ( !IsLocal || !_other.gameObject.activeSelf || owner is null )
        {
            return;
        }

        Actor defender = _other.GetComponent<Actor>();
        if ( defender is null )
        {
            return;
        }

        if ( penetrateCount.IsZero || alreadyHitActors.Contains( defender.Serial ) )
        {
            return;
        }

        // Bullet���� �浹��
        if ( defender is Bullet bullet )
        {
            // ���� Ŭ�� ó����, �ߺ�ó���� �Ǿ Serial�� ���� �ʿ��� Hit��Ŵ
            if ( bullet.Serial > Serial )
            {
                return;
            }
            bullet.HitTarget( this, SyncType.LocalFirst );
        }

        HitTarget( defender, SyncType.LocalFirst );
    }

    protected override void OnParticleSystemStopped()
    {
        GameManager.Inst.PushRemoveActorToSend( Serial );
        Rigid2D.simulated = false;
    }
    #endregion

    public void Fire( in BULLET_SHOT_INFO _shotInfo, BULLET_INFO _bulletInfo )
    {
        IsLocal = _shotInfo.isLocal;
        Serial = _bulletInfo.serial;
        owner = GameManager.Inst.GetActor( _shotInfo.owner ) as Character;
        if ( IsLocal )
        {
            gameObject.layer = Global.Layer.PlayerAttack;
            Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Enemy
                | Global.LayerFlag.EnemyAttack
                | Global.LayerFlag.Wall
                | Global.LayerFlag.Misc );
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
        Rigid2D.simulated = true;

        transform.SetPositionAndRotation( _shotInfo.pos.To(), Quaternion.Euler( 0, 0, _bulletInfo.angle - 90 ) );
        Rigid2D.velocity = transform.up * ( data.moveSpeed * _bulletInfo.rate );

        OnFireEvent?.Invoke( this );
    }
    
    public float GetInitHp()
    {
        return data.penetratePower * 20f;
    }

    #region Implement IHitable
    public void HitTarget( Actor _defender, SyncType _syncType, float _serverHp = 0f )
    {
        if ( _syncType != SyncType.LocalEcho )
        {
            OnHitEvent?.Invoke( this );
        }
        _defender?.OnHit( owner, this, _syncType, _serverHp );

        if ( _syncType == SyncType.LocalFirst )
        {
            alreadyHitActors.Add( _defender.Serial );
            if ( !( _defender is Bullet ) )
            {
                penetrateCount.Current -= _defender.PenetrationResist;
                if ( penetrateCount.IsZero )
                {
                    // ���� Release�� ���� ����(LocalEcho)�� �ް� �Ѵ�
                    Rigid2D.simulated = false;
                }
            }

            HIT_INFO hit;
            hit.needRelease = penetrateCount.IsZero;
            hit.hiter = Serial;
            hit.attacker = owner.Serial;
            hit.defender = _defender.Serial;
            hit.pos = new VECTOR2( transform.position );
            // Client->Server == Damage, Server->Client == Hp
            hit.hp = GetDamage();
            GameManager.Inst.PushHitInfoToSend( hit );
        }
    }

    public float GetDamage()
    {
        return totalDamage;
    }

    public Vector2 GetUpDirection()
    {
        return transform.up;
    }

    public Vector2 GetPushingForce()
    {
        return transform.up * data.pushingPower;
    }
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyableActor : Actor
{
    [SerializeField]
    private float maxHp;
    [SerializeField]
    private int penetrationResist;
    [Header( "< Destroy >" ), SerializeField]
    private float deadDuration;
    [SerializeField]
    private AudioClip destroySound;
    [Serializable]
    public struct SpawnInfo
    {
        public float ratio;
        public Actor actor;
    }
    [SerializeField]
    private List<SpawnInfo> spawnObjects;
    [SerializeField]
    private UnityEvent<Actor/*attacker*/> destroyEvent;

    private bool isDead;
    private bool prevIsSleep = true;
    protected Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();

        Hp.Current = Hp.Max = maxHp;
        PenetrationResist = penetrationResist;
    }

    protected virtual void FixedUpdate()
    {
        if ( !GameManager.Inst.IsHost() )
        {
            return;
        }

        bool isStopped = !prevIsSleep && Rigid2D.IsSleeping();
        if ( isStopped )
        {
            MOVEMENT_INFO protocol;
            protocol.serial = Serial;
            protocol.pos = new VECTOR2( Rigid2D.position );
            protocol.vel = new VECTOR2( Vector2.zero );
            Network.Inst.Send( PacketType.SYNC_MOVEMENT_REQ, protocol );
        }

        prevIsSleep = Rigid2D.IsSleeping();
    }

    protected override void OnDead( Actor _attacker, IHitable _hitable ) 
    {
        if ( isDead )
        {
            return;
        }

        isDead = true;
        gameObject.layer = Global.Layer.Invincible;
        Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Wall | Global.LayerFlag.Void );
        if ( _hitable is not null )
        {
            Rigid2D.AddForce( _hitable.GetPushingForce() * 2f );
        }

        Spriter.sortingOrder = -2;
        animator.SetBool( AnimatorParameters.IsDestroy, true );
        AudioManager.Inst.Play( destroySound, transform.position );
        StartCoroutine( RemoveDead( deadDuration ) );
        destroyEvent?.Invoke( _attacker );
    }

    private IEnumerator RemoveDead( float _duration )
    {
        if ( _duration <= 0f )
        {
            yield break;
        }

        yield return YieldCache.WaitForSeconds( _duration );
        Release();
    }

    #region DeadEvents
    public void DeadExplode( Actor _attacker )
    {
        if ( _attacker is null || !_attacker.IsLocal )
        {
            return;
        }

        List<Actor> targets = GetSpawnObjects();
        foreach ( Actor target in targets )
        {
            Bullet bullet = target as Bullet;
            if ( bullet is null )
            {
                Debug.LogWarning( $"DeadObject is not Bullet. {target.name}" );
                return;
            }

            BULLET_SHOT_INFO protocol;
            protocol.isLocal = false;
            protocol.isExplode = true;
            protocol.prefab = GameManager.Inst.GetPrefabIndex( bullet );
            protocol.pos = new VECTOR2( transform.position );
            protocol.look = 0f;
            protocol.owner = _attacker.Serial;
            protocol.damage = bullet.data.damage;
            protocol.hp = bullet.GetInitHp();
            protocol.bullets = new List<BULLET_INFO>();

            float angle = Global.GetAngle( _attacker.transform.position, transform.position );
            BULLET_INFO bulletInfo;
            bulletInfo.angle = angle;
            bulletInfo.serial = 0;
            bulletInfo.rate = 1f;
            protocol.bullets.Add( bulletInfo );
            Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, protocol );
        }
    }

    public void SpawnObject( Actor _attacker )
    {
        if ( _attacker is null || !_attacker.IsLocal )
        {
            return;
        }

        // 여러개 스폰시 겹쳐보여서 랜덤위치 부여
        float posDelta = 0;
        Vector2 randDirection = new Vector2( UnityEngine.Random.value - .5f, UnityEngine.Random.value - .5f ).normalized;

        List<Actor> targets = GetSpawnObjects();
        foreach ( Actor target in targets )
        {
            ACTOR_INFO protocol;
            protocol.isLocal = false;
            protocol.prefab = GameManager.Inst.GetPrefabIndex( target );
            protocol.serial = 0;
            protocol.pos = new VECTOR2( ( Vector2 )transform.position + posDelta * randDirection );
            protocol.vel = new VECTOR2( Vector2.zero );
            protocol.hp = 1f;
            protocol.inter = 0f;
            Network.Inst.Send( PacketType.SPAWN_ACTOR_REQ, protocol );
            posDelta += 1f;
        }
    }

    private List<Actor> GetSpawnObjects()
    {
        List<Actor> results = new List<Actor>();
        foreach ( SpawnInfo info in spawnObjects )
        {
            if ( info.ratio >= UnityEngine.Random.value )
            {
                results.Add( info.actor );
            }
        }

        return results;
    }
    #endregion
}

public static partial class AnimatorParameters
{
    public static int IsDestroy = Animator.StringToHash( "IsDestroy" );
}

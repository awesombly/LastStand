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
    private float deadDuration;
    [SerializeField]
    private int penetrationResist;
    [SerializeField]
    private Bullet deadObject;
    [SerializeField]
    private UnityEvent<Actor/*attacker*/> deadEvent;

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
        Rigid2D.excludeLayers = ~( int )Global.LayerFlag.Wall;
        if ( _hitable is not null )
        {
            Rigid2D.AddForce( _hitable.GetPushingForce() * 2f );
        }

        Spriter.sortingOrder = -2;
        animator.SetBool( AnimatorParameters.IsDestroy, true );
        StartCoroutine( RemoveDead( deadDuration ) );
        deadEvent?.Invoke( _attacker );
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

        BULLET_SHOT_INFO protocol;
        protocol.isLocal = false;
        protocol.isExplode = true;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( deadObject );
        protocol.pos = new VECTOR2( transform.position );
        protocol.look = 0f;
        protocol.owner = _attacker.Serial;
        protocol.damage = deadObject.data.damage;
        protocol.hp = deadObject.GetInitHp();
        protocol.bullets = new List<BULLET_INFO>();

        float angle = Global.GetAngle( _attacker.transform.position, transform.position );
        BULLET_INFO bulletInfo;
        bulletInfo.angle = angle;
        bulletInfo.serial = 0;
        bulletInfo.rate = 1f;
        protocol.bullets.Add( bulletInfo );
        Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, protocol );
    }
    #endregion
}

public static partial class AnimatorParameters
{
    public static int IsDestroy = Animator.StringToHash( "IsDestroy" );
}

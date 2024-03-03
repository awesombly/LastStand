using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableActor : Actor
{
    [SerializeField]
    private float maxHp;
    [SerializeField]
    private float deadDuration;
    [SerializeField]
    private int penetrationResist;
    private bool isDead;
    protected Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        Hp.Current = Hp.Max = maxHp;
        PenetrationResist = penetrationResist;
    }

    public override void SetMovement( Vector3 _position, Vector3 _velocity ) { }

    public override void OnHit( Actor _attacker, Bullet _bullet ) 
    {
        base.OnHit( _attacker, _bullet );
    }

    protected override void OnDead( Actor _attacker, Bullet _bullet ) 
    {
        if ( isDead )
        {
            return;
        }

        isDead = true;
        gameObject.layer = Global.Layer.Invincible;
        Rigid2D.excludeLayers = ~( int )Global.LayerFlag.Wall;
        Rigid2D.AddForce( _bullet.transform.up * _bullet.data.pushingPower * 2f );
        animator.SetBool( AnimatorParameters.IsDestroy, true );
        StartCoroutine( RemoveDead( deadDuration ) );
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
}

public static partial class AnimatorParameters
{
    public static int IsDestroy = Animator.StringToHash( "IsDestroy" );
}

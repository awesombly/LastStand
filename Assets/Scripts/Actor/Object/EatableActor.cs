using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EatableActor : Actor
{
    [Header( "< Eat >" ), SerializeField]
    private float autoEatRadius;
    [SerializeField]
    private float initForcePower;
    [SerializeField]
    private float followSpeed;
    [SerializeField]
    private float eatParameter;
    [SerializeField]
    private UnityEvent<Actor/*target*/, float/*parameter*/> eatEvent;

    private Player followTarget = null;
    private float curFollowSpeed = 0f;

    private void OnEnable()
    {
        followTarget = null;
        curFollowSpeed = 0f;
    }

    private void FixedUpdate()
    {
        if ( followTarget is not null )
        {
            // Follow Target
            curFollowSpeed += ( followSpeed * Time.fixedDeltaTime );
            Vector3 direction = ( ( Vector2 )followTarget.transform.position - Rigid2D.position ).normalized;
            transform.position += direction * curFollowSpeed * Time.fixedDeltaTime;
            return;
        }

        FindFollowTarget();
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        Player target = _other.GetComponent<Player>();
        if ( target is null )
        {
            return;
        }

        eatEvent?.Invoke( target, eatParameter );
        Release(); // Req
    }

    private void FindFollowTarget()
    {
        Player nearestTarget = null;
        float nearestDistance = float.MaxValue;
        RaycastHit2D[] hits = Physics2D.CircleCastAll( transform.position, autoEatRadius, Vector2.zero, 0f, ( int )Global.LayerFlag.Player );
        foreach ( RaycastHit2D hit in hits )
        {
            Player player = hit.transform.GetComponent<Player>();
            if ( player is null || !player.IsLocal )
            {
                continue;
            }

            float distance = Vector2.Distance( transform.position, player.transform.position );
            if ( nearestDistance < distance )
            {
                continue;
            }

            nearestTarget = player;
            nearestDistance = distance;
        }

        if ( nearestTarget is not null )
        {
            SetTarget( nearestTarget );
        }
    }

    public void SetTarget( Player _target )
    {
        followTarget = _target;
        if ( followTarget is null )
        {
            return;
        }

        Vector2 direction = new Vector2( Random.value - .5f, Random.value - .5f ).normalized;
        Rigid2D.AddForce( direction * initForcePower );

        // Req Protocol
    }

    public override void SetHp( float _hp, Actor _attacker, IHitable _hitable, SyncType _syncType )
    {
        Hp.Current = Hp.Max = _hp; 
    }

    public override void OnHit( Actor _attacker, IHitable _hitable, SyncType _syncType, float _serverHp = 0f ) { }

    protected override void OnDead( Actor _attacker, IHitable _hitable ) { }

    protected override void OnChangeLocal( bool _isLocal ) { }
}

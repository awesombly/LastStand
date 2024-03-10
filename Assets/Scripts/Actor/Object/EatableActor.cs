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
        if ( target is null || followTarget is null 
            || !ReferenceEquals( target, followTarget ) || !target.IsLocal )
        {
            return;
        }

        InvokeEatEvent( followTarget, eatParameter );

        // Req Protocol
        INTERACTION_INFO protocol;
        protocol.serial = Serial;
        protocol.target = followTarget.Serial;
        protocol.angle = eatParameter;
        protocol.pos = new VECTOR2( Vector2.zero );
        Network.Inst.Send( PacketType.SYNC_EATABLE_EVENT_REQ, protocol );
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
            Vector2 force = new Vector2( Random.value - .5f, Random.value - .5f ).normalized * initForcePower;
            SetTarget( nearestTarget, force );
        }
    }

    public void SetTarget( Player _target, Vector2 _force )
    {
        followTarget = _target;
        if ( followTarget is null )
        {
            return;
        }
        
        Rigid2D.AddForce( _force );

        // Req Protocol
        if ( followTarget.IsLocal )
        {
            INTERACTION_INFO protocol;
            protocol.serial = Serial;
            protocol.target = followTarget.Serial;
            protocol.angle = eatParameter;
            protocol.pos = new VECTOR2( _force );
            Network.Inst.Send( PacketType.SYNC_EATABLE_TARGET_REQ, protocol );
        }
    }

    public void InvokeEatEvent( Player _target, float _parameter )
    {
        eatEvent?.Invoke( _target, _parameter );
    }

    #region Eat Event
    public void EatHeart( Actor _target, float _value )
    {
        if ( _target is null )
        {
            Debug.LogWarning( $"Target is null. me:{name}" );
            return;
        }

        _target.SetHp( _target.Hp.Current + _value, null, null, SyncType.FromServer );
        Release();
    }
    #endregion

    public override void SetHp( float _hp, Actor _attacker, IHitable _hitable, SyncType _syncType )
    {
        Hp.Current = Hp.Max = _hp; 
    }

    public override void OnHit( Actor _attacker, IHitable _hitable, SyncType _syncType, float _serverHp = 0f ) { }

    protected override void OnDead( Actor _attacker, IHitable _hitable ) { }

    protected override void OnChangeLocal( bool _isLocal ) { }
}

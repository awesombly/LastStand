using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerDodgeAttack : MonoBehaviour, IHitable
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private float pushingPower;

    private Vector2 direction;
    private List<uint/*Serial*/> alreadyHitActors = new List<uint>();

    private Player player;
    private PlayerMovement movement;

    public event Action<Player/*attacker*/, Actor/*defender*/> OnHitEvent;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        movement = GetComponentInParent<PlayerMovement>();

        movement.OnDodgeAction += OnDodgeAction;

        gameObject.SetActive( false );
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        Actor defender = _other.GetComponent<Actor>();
        if ( defender == null )
        {
            Debug.LogWarning( $"Actor is null. other:{_other.name}" );
            return;
        }

        if ( alreadyHitActors.Contains( defender.Serial ) )
        {
            return;
        }

        HitTarget( defender, SyncType.LocalFirst );
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction, float _duration )
    {
        direction = _direction;

        if ( !player.IsLocal )
        {
            return;
        }
        gameObject.SetActive( _isActive );
        alreadyHitActors.Clear();
    }

    #region Implement IHitable
    public void HitTarget( Actor _defender, SyncType _syncType, float _serverHp = 0f )
    {
        if ( _syncType != SyncType.LocalEcho )
        {
            OnHitEvent?.Invoke( player, _defender );
        }
        _defender?.OnHit( player, this, _syncType, _serverHp );

        if ( _syncType == SyncType.LocalFirst )
        {
            alreadyHitActors.Add( _defender.Serial );

            HIT_INFO hit;
            hit.needRelease = false;
            hit.hiter = 0;
            hit.attacker = player.Serial;
            hit.defender = _defender.Serial;
            hit.pos = new VECTOR2( transform.position );
            // Client->Server == Damage, Server->Client == Hp
            hit.hp = GetDamage();
            GameManager.Inst.PushHitInfoToSend( hit );
        }
    }

    public float GetDamage()
    {
        return damage * player.data.attackRate;
    }

    public Vector2 GetUpDirection()
    {
        return direction;
    }

    public Vector2 GetPushingForce()
    {
        return direction * pushingPower;
    }
    #endregion
}

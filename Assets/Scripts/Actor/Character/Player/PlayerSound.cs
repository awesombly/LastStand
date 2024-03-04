using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();

        var movement = GetComponent<PlayerMovement>();
        movement.OnDodgeAction += OnDodgeAction;

        player.DodgeAttack.OnHitEvent += OnDodgeHit;
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction, float _duration )
    {
        if ( _isActive )
        {
            AudioManager.Inst.Play( player.playerData.dodgeActionSound, player.transform.position );
        }
    }

    private void OnDodgeHit( Player _attacker, Actor _defender )
    {
        AudioManager.Inst.Play( player.playerData.dodgeHitSound, player.transform.position );
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Character : Actor
{
    public float Hp { get; set; }
    [SerializeField]
    protected CharacterData data;

    public Action<Character/*dead*/, Character/*attacker*/> OnDeadEvent;

    public Rigidbody2D Rigid2D { get; private set; }

    protected virtual void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
        Hp = data.maxHp;
    }

    public virtual void OnHit( Character _attacker, float _damage, Vector2 _force )
    {
        if ( _force != Vector2.zero )
        {
            Rigid2D.AddForce( _force );
        }

        Hp -= _damage;
        if ( Hp <= 0 )
        {
            OnDeadEvent?.Invoke( this, _attacker );
        }
    }
}

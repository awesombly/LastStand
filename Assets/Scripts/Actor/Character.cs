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

    protected virtual void Awake()
    {
        Hp = data.maxHp;
    }

    public virtual void HitAttack( Character _attacker, float _damage )
    {
        Hp -= _damage;
        if ( Hp <= 0 )
        {
            OnDeadEvent?.Invoke( this, _attacker );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Actor
{
    protected override void Awake()
    {
        base.Awake();
        Hp.Current = Hp.Max = 1000f;
        PenetrationResist = 1000;
    }

    public override void SetMovement( Vector3 _position, Vector3 _velocity ) { }

    public override void SetHp( float _hp, Actor _attacker, Bullet _bullet ) { }

    public override void OnHit( Actor _attacker, Bullet _bullet ) { }

    protected override void OnDead( Actor _attacker, Bullet _bullet ) { }

    protected override void OnChangeLocal( bool _isLocal ) { }
}

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

    public override void SetHp( float _hp, Actor _attacker, IHitable _hitable, SyncType _syncType ) { }

    public override void OnHit( Actor _attacker, IHitable _hitable, SyncType _syncType, float _serverHp = 0f ) { }

    protected override void OnDead( Actor _attacker, IHitable _hitable ) { }

    protected override void OnChangeLocal( bool _isLocal ) { }
}

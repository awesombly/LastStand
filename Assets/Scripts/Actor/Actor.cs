using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Actor : Poolable
{
    private bool isLocal = true;
    public bool IsLocal
    {
        get => isLocal;
        set { OnChangeLocal( isLocal = value ); }
    }

    private uint serial = uint.MaxValue;
    public uint Serial
    {
        get => serial; 
        set
        {
            serial = value;
            name = name + ":" + serial;
            // Serial을 부여받아야 등록가능
            GameManager.Inst.RegistActor( this );
        }
    }
    // 씬에 미리 배치된 Actor 구별용
    public int MyHashCode { get; private set; }

    [HideInInspector]
    public Global.StatusFloat Hp;
    public int PenetrationResist { get; protected set; }

    public Rigidbody2D Rigid2D { get; private set; }

    #region Unity Callback
    protected virtual void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
        MyHashCode = name.GetHashCode();
        PenetrationResist = 1;
    }

    protected virtual void OnDisable()
    {
        GameManager.Inst.UnregistActor( serial );
    }
    #endregion

    public virtual void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        Rigid2D.velocity = _velocity;
    }

    public virtual void SetHp( float _hp, Actor _attacker, IHitable _hitable )
    {
        Hp.Current = _hp;
        if ( Hp.IsZero && gameObject.activeSelf )
        {
            OnDead( _attacker, _hitable );
        }
    }

    public virtual void OnHit( Actor _attacker, IHitable _hitable )
    {
        if ( _attacker == null || _hitable == null )
        {
            Debug.LogWarning( $"Actor is null. attacker:{_attacker}, hitable:{_hitable}" );
            return;
        }

        Rigid2D.AddForce( _hitable.GetPushingForce() );

        SetHp( Hp.Current - _hitable.GetDamage(), _attacker, _hitable );
    }

    protected virtual void OnDead( Actor _attacker, IHitable _hitable )
    {
        Release();
    }

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

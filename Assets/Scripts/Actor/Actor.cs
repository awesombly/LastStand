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

    [HideInInspector]
    public Global.StatusFloat Hp;

    public Rigidbody2D Rigid2D { get; private set; }

    #region Unity Callback
    protected virtual void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnDisable()
    {
        GameManager.Inst.UnregistActor( serial );
    }
    #endregion

    public virtual void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        if ( Rigid2D != null )
        {
            Rigid2D.velocity = _velocity;
        }
    }

    public virtual void OnHit( Actor _attacker, Bullet _bullet )
    {
        if ( _attacker == null || _bullet == null )
        {
            Debug.LogWarning( $"Actor is null. attacker:{_attacker}, bullet:{_bullet}" );
            return;
        }

        Vector2 force = _bullet.data.pushingPower * _bullet.transform.up;
        Rigid2D.AddForce( force );

        Hp.Current -= _bullet.GetDamage();
        if ( Hp.IsZero )
        {
            OnDead( _attacker, _bullet );
        }
    }

    protected virtual void OnDead( Actor _attacker, Bullet _bullet )
    {
    }

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

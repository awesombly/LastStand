using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Actor : MonoBehaviour
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
            if ( serial == value )
            {
                Debug.LogWarning( "Is same serial : " + serial );
                return;
            }

            serial = value;
            name = name + ":" + serial;
            GameManager.Inst.RegistActor( this );
        }
    }

    public Rigidbody2D Rigid2D { get; private set; }
    private IObjectPool<Actor> parentPool = null;

    protected virtual void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
    }

    public virtual void Release()
    {
        if ( parentPool == null )
        {
            Destroy( gameObject );
            return;
        }

        if ( gameObject.activeSelf )
        {
            GameManager.Inst.UnregistActor( serial );
            parentPool.Release( this );
        }
    }

    public virtual void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        if ( Rigid2D != null )
        {
            Rigid2D.velocity = _velocity;
        }
    }

    public void SetPool( IObjectPool<Actor> _pool )
    {
        parentPool = _pool;
    }

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

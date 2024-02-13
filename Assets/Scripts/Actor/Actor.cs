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
            GameManager.Inst.RegistActor( this );
        }
    }

    private IObjectPool<Actor> parentPool = null;

    public virtual void SetMovement( Vector3 _position, Quaternion _rotation, Vector3 _velocity )
    {
        transform.SetPositionAndRotation( _position, _rotation );
        var rigid = GetComponent<Rigidbody2D>();
        if ( rigid != null )
        {
            rigid.velocity = _velocity;
        }
    }

    public void Release()
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

    public void SetPool( IObjectPool<Actor> _pool )
    {
        parentPool = _pool;
    }

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

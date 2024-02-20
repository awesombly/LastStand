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
            if ( serial == value )
            {
                Debug.LogWarning( "Is same serial : " + serial );
                return;
            }

            serial = value;
            name = name + ":" + serial;
            // Serial을 부여받아야 등록가능
            GameManager.Inst.RegistActor( this );
        }
    }

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

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolObject : MonoBehaviour
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
            GameManager.Inst.RegistObject( this );
        }
    }

    private IObjectPool<PoolObject> parentPool = null;

    public void Release()
    {
        if ( parentPool == null )
        {
            Destroy( gameObject );
            return;
        }

        if ( gameObject.activeSelf )
        {
            GameManager.Inst.UnRegistObject( serial );
            parentPool.Release( this );
        }
    }

    public void SetPool( IObjectPool<PoolObject> _pool )
    {
        parentPool = _pool;
    }

    protected virtual void OnChangeLocal( bool _isLocal )
    {
    }
}

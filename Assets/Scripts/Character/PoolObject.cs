using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolObject : MonoBehaviour
{
    private IObjectPool<PoolObject> parentPool = null;

    public void Release()
    {
        if ( parentPool == null )
        {
            Debug.LogError( "parentPool is null : " + gameObject.name );
            return;
        }

        parentPool.Release( this );
    }

    public void SetPool( IObjectPool<PoolObject> _pool )
    {
        parentPool = _pool;
    }
}

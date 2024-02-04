using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolObject : MonoBehaviour
{
    private IObjectPool<PoolObject> parentPool = null;

    public void Release()
    {
        parentPool?.Release( this );
    }

    public void SetPool( IObjectPool<PoolObject> _pool )
    {
        parentPool = _pool;
    }
}

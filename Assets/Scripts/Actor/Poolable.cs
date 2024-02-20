using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Poolable : MonoBehaviour
{
    private IObjectPool<Poolable> parentPool = null;

    private void OnParticleSystemStopped()
    {
        Release();
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
            parentPool.Release( this );
        }
    }

    public void SetPool( IObjectPool<Poolable> _pool )
    {
        parentPool = _pool;
    }
}

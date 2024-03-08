using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Poolable : MonoBehaviour
{
    public int PrefabIndex { get; set; } = -1;
    private IObjectPool<Poolable> parentPool = null;

    public SpriteRenderer Spriter { get; private set; } = null;

    protected virtual void Awake()
    {
        Spriter = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnParticleSystemStopped()
    {
        Release();
    }

    public virtual void Release()
    {
        if ( parentPool is null )
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

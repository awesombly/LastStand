using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<Poolable/*Prefab*/, IObjectPool<Poolable>> pools = new Dictionary<Poolable, IObjectPool<Poolable>>();
    private Dictionary<Poolable/*Prefab*/, GameObject/*Parent*/> poolParents = new Dictionary<Poolable, GameObject>();
    private Poolable curPrefab = null;    // GameObject 생성시 프리팹 구별이 안되서 추가

    protected override void Awake()
    {
        base.Awake();
        SceneBase.OnBeforeSceneLoad += Clear;
    }

    public Poolable Get( Poolable _prefab )
    {
        if ( !pools.ContainsKey( _prefab ) )
        {
            RegisteObject( _prefab );
        }

        curPrefab = _prefab;
        Poolable poolable = pools[_prefab].Get();
        poolable.PrefabIndex = curPrefab.PrefabIndex;
        return poolable;
    }

    public Poolable Get( int _prefabIndex )
    {
        Poolable prefab = GameManager.Inst.GetPrefab( _prefabIndex );
        if ( !pools.ContainsKey( prefab ) )
        {
            RegisteObject( prefab );
        }

        curPrefab = prefab;
        Poolable poolable = pools[prefab].Get();
        if ( ReferenceEquals( poolable, null ) )
        {
            Debug.LogError( $"Invalid prefab. prefab:{prefab}" );
            return null;
        }

        poolable.PrefabIndex = _prefabIndex;
        return poolable;
    }

    private void RegisteObject( Poolable _prefab )
    {
        if ( pools.ContainsKey( _prefab ) )
        {
            Debug.Log( "Aleady Exist Object." );
            return;
        }

        var parentObject = new GameObject( _prefab.name );
        parentObject.transform.SetParent( transform );
        poolParents[_prefab] = parentObject;

        int initCapacity = 10;
        curPrefab = _prefab;
        curPrefab.PrefabIndex = GameManager.Inst.GetPrefabIndex( curPrefab );
        pools.Add( curPrefab, new ObjectPool<Poolable>( OnCreate, OnGetAction, OnReleaseAction, OnDestroyAction, true/*checkError*/, initCapacity ) );
    }

    private void Clear()
    {
        foreach ( var actorPool in pools )
        {
            actorPool.Value.Clear();
        }
        pools.Clear();

        foreach ( var item in poolParents )
        {
            Destroy( item.Value );
        }
        poolParents.Clear();

        curPrefab = null;
    }

    #region ObjectPool Functions
    private Poolable OnCreate()
    {
        Poolable poolable = Instantiate( curPrefab, poolParents[curPrefab].transform );
        if ( poolable is null )
        {
            Debug.LogError( "Not found Prefab : " + curPrefab );
            return null;
        }

        if ( !pools.ContainsKey( curPrefab ) )
        {
            Debug.LogError( "Not exist pool : " + curPrefab );
            return null;
        }

        poolable.SetPool( pools[curPrefab] );

        return poolable;
    }

    private void OnGetAction( Poolable _actor )
    {
        _actor.gameObject.SetActive( true );
    }

    private void OnReleaseAction( Poolable _actor )
    {
        _actor.gameObject.SetActive( false );
    }

    private void OnDestroyAction( Poolable _actor )
    {
        Destroy( _actor.gameObject );
    }
    #endregion
}

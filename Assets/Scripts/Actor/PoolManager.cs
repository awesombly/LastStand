using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject/*Prefab*/, IObjectPool<Poolable>> pools = new Dictionary<GameObject, IObjectPool<Poolable>>();
    private Dictionary<GameObject/*Prefab*/, GameObject/*Parent*/> poolParents = new Dictionary<GameObject, GameObject>();
    private GameObject curPrefab = null;    // GameObject 생성시 프리팹 구별이 안되서 추가

    protected override void Awake()
    {
        base.Awake();
        SceneBase.OnBeforeSceneLoad += Clear;
    }

    public Poolable Get( GameObject _prefab )
    {
        if ( !pools.ContainsKey( _prefab ) )
        {
            RegisteObject( _prefab );
        }

        curPrefab = _prefab;
        return pools[_prefab].Get();
    }

    public Poolable Get( int _prefabIndex )
    {
        return Get( GameManager.Inst.GetPrefab( _prefabIndex ) );
    }

    private void RegisteObject( GameObject _prefab )
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
        GameObject go = Instantiate( curPrefab, poolParents[curPrefab].transform );

        var poolable = go.GetComponent<Poolable>();
        if ( poolable == null )
        {
            Debug.LogError( "Not have Poolable : " + go.name );
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

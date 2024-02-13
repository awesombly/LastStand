using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject/*Prefab*/, IObjectPool<Actor>> pools = new Dictionary<GameObject, IObjectPool<Actor>>();
    private Dictionary<GameObject/*Prefab*/, GameObject/*Parent*/> poolParents = new Dictionary<GameObject, GameObject>();
    private GameObject curPrefab;       // GameObject 생성시 프리팹 구별이 안되서 추가

    public Actor Get( GameObject _prefab )
    {
        if ( !pools.ContainsKey( _prefab ) )
        {
            RegisterObject( _prefab );
        }

        curPrefab = _prefab;
        return pools[_prefab].Get();
    }

    public Actor Get( int _prefabIndex )
    {
        return Get( GameManager.Inst.GetPrefab( _prefabIndex ) );
    }

    private void RegisterObject( GameObject _prefab )
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
        pools.Add( curPrefab, new ObjectPool<Actor>( CreateActor, OnGetAction, OnReleaseAction, OnDestroyAction, true/*checkError*/, initCapacity ) );
    }

    #region ObjectPool Functions
    private Actor CreateActor()
    {
        GameObject go = Instantiate( curPrefab, poolParents[curPrefab].transform );

        var actor = go.GetComponent<Actor>();
        if ( actor == null )
        {
            Debug.LogError( "Not found Actor : " + go.name );
            return null;
        }

        if ( !pools.ContainsKey( curPrefab ) )
        {
            Debug.LogError( "Not exist pool : " + curPrefab );
            return null;
        }

        actor.SetPool( pools[curPrefab] );

        return actor;
    }

    private void OnGetAction( Actor _actor )
    {
        _actor.gameObject.SetActive( true );
    }

    private void OnReleaseAction( Actor _actor )
    {
        _actor.gameObject.SetActive( false );
    }

    private void OnDestroyAction( Actor _actor )
    {
        Destroy( _actor.gameObject );
    }
    #endregion
}

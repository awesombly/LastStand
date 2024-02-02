using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject/*Prefab*/, IObjectPool<PoolObject>> pools = new Dictionary<GameObject, IObjectPool<PoolObject>>();
    private GameObject curPrefab;

    public PoolObject Get( GameObject _prefab )
    {
        if ( pools.ContainsKey( _prefab ) == false )
        {
            RegisterObject( _prefab );
        }

        curPrefab = _prefab;
        return pools[_prefab].Get();
    }

    public void Release( PoolObject _object )
    {
        // ��ϵ� pool�� ã�⿣ ����� ��� PoolObject ���ο��� ó��
        _object.Release();
    }

    private void RegisterObject( GameObject _prefab )
    {
        if ( pools.ContainsKey( _prefab ) == true )
        {
            Debug.Log( "Aleady Exist Object." );
            return;
        }

        int initCapacity = 10;
        curPrefab = _prefab;
        pools.Add( curPrefab, new ObjectPool<PoolObject>( CreatePoolObject, OnGetAction, OnReleaseAction, OnDestroyAction, true/*checkError*/, initCapacity ) );
    }

    #region ObjectPool Functions
    private PoolObject CreatePoolObject()
    {
        GameObject go = Instantiate( curPrefab, gameObject.transform );

        var poolObject = go.GetComponent<PoolObject>();
        if ( poolObject == null )
        {
            Debug.LogError( "Not found PoolObject : " + go.name );
            return null;
        }

        if ( pools.ContainsKey( curPrefab ) == false )
        {
            Debug.LogError( "Not exist pool : " + curPrefab );
            return null;
        }

        poolObject.SetPool( pools[curPrefab] );

        return poolObject;
    }

    private void OnGetAction( PoolObject _object )
    {
        _object.gameObject.SetActive( true );
    }

    private void OnReleaseAction( PoolObject _object )
    {
        _object.gameObject.SetActive( false );
    }

    private void OnDestroyAction( PoolObject _object )
    {
        Destroy( _object.gameObject );
    }
    #endregion
}
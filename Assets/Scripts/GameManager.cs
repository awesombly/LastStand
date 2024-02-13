using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector]
    public Player localPlayer;

    [SerializeField]    // PoolManager를 사용할 모든 프리팹들
    private List<GameObject/*Prefab*/> prefabList;

    private Dictionary<uint/*Serial*/, PoolObject> objects = new Dictionary<uint, PoolObject>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void RegistObject( PoolObject _object )
    {
        if ( _object == null
            || objects.ContainsKey( _object.Serial ) )
        {
            Debug.LogWarning( "Invalid Object : " + _object );
        }

        objects[_object.Serial] = _object;
    }

    public void UnRegistObject( uint _serial )
    {
        if ( _serial == uint.MaxValue )
        {
            return;
        }

        if ( !objects.Remove( _serial ) )
        {
            Debug.LogWarning( "Invalid Serial : " + _serial );
        }
    }

    public int GetPrefabIndex( GameObject _prefab )
    {
        int index = prefabList.FindIndex( ( _item ) => _item == _prefab );
        if ( index == -1 )
        {
            Debug.LogError( "Prefab not found : " + _prefab );
        }

        return index;
    }

    public GameObject GetPrefab( int _index )
    {
        if ( prefabList.Count <= _index || _index < 0 )
        {
            Debug.LogError( $"Invalid index : {_index}, prefabCount : {prefabList.Count}" );
            return null;
        }

        return prefabList[_index];
    }
}

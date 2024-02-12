using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Player localPlayer;

    [SerializeField]
    private List<GameObject/*Prefab*/> prefabList;

    public Dictionary<uint/*Serial*/, PoolObject> objects;

    protected override void Awake()
    {
        base.Awake();
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

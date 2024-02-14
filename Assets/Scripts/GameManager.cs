using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector]
    public Player localPlayer;

    [SerializeField]    // PoolManager를 사용할 모든 프리팹들
    private List<GameObject/*Prefab*/> prefabList;

    private Dictionary<uint/*Serial*/, Actor> actors = new Dictionary<uint, Actor>();

    protected override void Awake()
    {
        base.Awake();
        SceneBase.OnBeforeSceneLoad += Clear;
    }

    public void RegistActor( Actor _actor )
    {
        if ( _actor == null
            || actors.ContainsKey( _actor.Serial ) )
        {
            Debug.LogWarning( "Invalid Actor : " + _actor );
        }

        actors[_actor.Serial] = _actor;
    }

    public void UnregistActor( uint _serial )
    {
        if ( _serial == uint.MaxValue )
        {
            return;
        }

        if ( !actors.Remove( _serial ) )
        {
            Debug.LogWarning( "Invalid Serial : " + _serial );
        }
    }

    public Actor GetActor( uint _serial )
    {
        if ( !actors.ContainsKey( _serial ) )
        {
            Debug.LogWarning( "Invalid Serial : " + _serial );
            return null;
        }

        return actors[_serial];
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

    private void Clear()
    {
        actors.Clear();
    }
}

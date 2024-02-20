using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static Vector3 MouseWorldPos { get; private set; }
    public static float LookAngle { get; private set; }
    public static Player LocalPlayer { get; set; }

    [SerializeField]
    private GameMangerSO data;

    private Dictionary<uint/*Serial*/, Actor> actors = new Dictionary<uint, Actor>();

    protected override void Awake()
    {
        base.Awake();
        SceneBase.OnBeforeSceneLoad += Clear;
    }

    private void Update()
    {
        MouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        if ( !ReferenceEquals( LocalPlayer, null ) )
        {
            LookAngle = Global.GetAngle( LocalPlayer.transform.position, MouseWorldPos );
        }
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

    public int GetPrefabIndex( Poolable _prefab )
    {
        int index = data.prefabList.FindIndex( ( _item ) => _item == _prefab );
        if ( index == -1 )
        {
            Debug.LogError( "Prefab not found : " + _prefab );
        }

        return index;
    }

    public Poolable GetPrefab( int _index )
    {
        if ( data.prefabList.Count <= _index || _index < 0 )
        {
            Debug.LogError( $"Invalid index : {_index}, prefabCount : {data.prefabList.Count}" );
            return null;
        }

        return data.prefabList[_index];
    }

    public Player GetPlayerPrefab()
    {
        return data.playerPrefab;
    }

    private void Clear()
    {
        LocalPlayer = null;
        actors.Clear();
    }
}

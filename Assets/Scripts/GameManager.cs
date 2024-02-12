using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class GameManager : Singleton<GameManager>
{
    public Player localPlayer;

    public List<GameObject/*Prefab*/> prefabList;

    protected override void Awake()
    {
        base.Awake();
        ProtocolSystem.Inst.Regist( SPAWN_ENEMY_ACK, AckSpawnEnemy );
    }

    public void SpawnEnemy( GameObject _prefab, Vector2 _position, Quaternion _rotation )
    {
        SPAWN_ENEMY protocol;
        protocol.prefab = prefabList.FindIndex( ( _item ) => _item == _prefab );
        protocol.serial = 0;
        protocol.x = _position.x;
        protocol.y = _position.y;
        Network.Inst.Send( new Packet( SPAWN_ENEMY_REQ, protocol ) );
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<SPAWN_ENEMY>( _packet );
        Enemy po = PoolManager.Inst.Get( prefabList[data.prefab] ) as Enemy;
        po.serial = (uint)data.serial;
        po.Initialize( new Vector3( data.x, data.y, 0f ) );
    }
}

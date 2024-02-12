using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class InGameScene : MonoBehaviour
{
    private void Awake()
    {
        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK, AckSpawnEnemy );
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Enemy enemy = PoolManager.Inst.Get( GameManager.Inst.GetPrefab( data.prefab ) ) as Enemy;
        enemy.serial = data.serial;
        enemy.Initialize( new Vector3( data.position.x, data.position.y, data.position.z ) );
    }
}

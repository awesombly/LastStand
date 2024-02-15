using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class InGameScene : SceneBase
{
    [SerializeField]
    private Transform spawnTransform;
    [SerializeField]
    private GameObject playerPrefab;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame;
        if ( spawnTransform == null )
        {
            spawnTransform = transform;
        }

        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK, AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK, AckSpawnEnemy );
        ProtocolSystem.Inst.Regist( SYNK_MOVEMENT_ACK, AckSynkMovement );
    }

    protected override void Start()
    {
        base.Start();
        InitLocalPlayer();
        ReqInGameLoadData();
        //ReqSpawnPlayer();
    }

    private void InitLocalPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if ( player == null )
        {
            return;
        }

        player.IsLocal = true;
        GameManager.Inst.localPlayer = player;
    }

    #region Req Protocols

    private void ReqInGameLoadData()
    {
        // 立加矫 积己且 Player 沥焊
        ACTOR_INFO protocol;
        protocol.socket = 0;
        protocol.isLocal = true;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
        protocol.serial = 0;
        protocol.position = new VECTOR3( spawnTransform.position + Vector3.one * Random.Range( -5f, 5f ) );
        protocol.rotation = new QUATERNION( spawnTransform.rotation );
        protocol.velocity = new VECTOR3( Vector3.zero );

        Network.Inst.Send( INGAME_LOAD_DATA_REQ, protocol );
    }
    #endregion

    #region Ack Protocols
    private void AckSpawnPlayer( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Player player = null;
        if ( GameManager.Inst.localPlayer != null &&
            ( GameManager.Inst.localPlayer.Serial == data.serial || data.isLocal ) )
        {
            player = GameManager.Inst.localPlayer;
            player.IsLocal = true;
        }
        else
        {
            player = PoolManager.Inst.Get( data.prefab ) as Player;
            player.IsLocal = false;
        }

        player.Serial = data.serial;
        player.transform.SetPositionAndRotation( data.position.To(), data.rotation.To() );
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Enemy enemy = PoolManager.Inst.Get( data.prefab ) as Enemy;
        enemy.Serial = data.serial;
        enemy.Initialize( new Vector3( data.position.x, data.position.y, data.position.z ) );
    }

    private void AckSynkMovement( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor.SetMovement( data.position.To(), data.rotation.To(), data.velocity.To() );
    }
    #endregion
}

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
        ProtocolSystem.Inst.Regist( INGAME_LOAD_DATA_ACK, AckInGameLoadData );
    }

    protected override void Start()
    {
        base.Start();
        InitLocalPlayer();
        ReqInGameLoadData();
        ReqSpawnPlayer();
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
    private void ReqSpawnPlayer()
    {
        ACTOR_INFO protocol;
        protocol.socket = 0;
        protocol.isLocal = true;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
        protocol.serial = 0;
        protocol.position = new VECTOR3( spawnTransform.position + Vector3.one * Random.Range( -5f, 5f ) );
        protocol.rotation = new QUATERNION( spawnTransform.rotation );
        protocol.velocity = new VECTOR3( Vector3.zero );
        Network.Inst.Send( SPAWN_PLAYER_REQ, protocol );
    }

    private void ReqInGameLoadData()
    {
        Network.Inst.Send( INGAME_LOAD_DATA_REQ, new EMPTY() );
    }
    #endregion

    #region Ack Protocols
    private void AckSpawnPlayer( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Player player = null;
        if ( data.isLocal && GameManager.Inst.localPlayer != null )
        {
            player = GameManager.Inst.localPlayer;
        }
        else
        {
            player = PoolManager.Inst.Get( data.prefab ) as Player;
        }

        player.IsLocal = data.isLocal;
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

    private void AckInGameLoadData( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );

        foreach ( Actor actor in GameManager.Inst.Actors.Values )
        {
            if ( !( actor is Player ) )
            {
                continue;
            }

            ACTOR_INFO protocol;
            protocol.socket = data.socket;
            protocol.isLocal = false;
            protocol.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
            protocol.serial = actor.Serial;
            protocol.position = new VECTOR3( actor.transform.position );
            protocol.rotation = new QUATERNION( actor.transform.rotation );
            protocol.velocity = new VECTOR3( Vector3.zero );
            Network.Inst.Send( SPAWN_PLAYER_REQ, protocol );
        }
    }
    #endregion
}

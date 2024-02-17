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

        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK,    AckSpawnEnemy );
        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK,   AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_BULLET_ACK,   AckSpawnBullet );
        ProtocolSystem.Inst.Regist( REMOVE_ACTOR_ACK,   AckRemoveActor );
        ProtocolSystem.Inst.Regist( SYNK_MOVEMENT_ACK,  AckSynkMovement );
        ProtocolSystem.Inst.Regist( SYNK_RELOAD_ACK,    AckSynkReload );
        ProtocolSystem.Inst.Regist( HIT_ACTOR_ACK,      AckHitActor );
    }

    protected override void Start()
    {
        base.Start();
        InitLocalPlayer();
        ReqInGameLoadData();
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
        PLAYER_INFO protocol;
        protocol.actorInfo.isLocal = true;
        protocol.actorInfo.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.position = new VECTOR3( spawnTransform.position + Vector3.one * Random.Range( -5f, 5f ) );
        protocol.actorInfo.rotation = new QUATERNION( spawnTransform.rotation );
        protocol.actorInfo.velocity = new VECTOR3( Vector3.zero );
        protocol.nickname = string.Empty;

        Network.Inst.Send( INGAME_LOAD_DATA_REQ, protocol );
    }
    #endregion

    #region Ack Protocols
    private void AckSpawnPlayer( Packet _packet )
    {
        var data = Global.FromJson<PLAYER_INFO>( _packet );
        Player player = null;
        if ( GameManager.Inst.localPlayer != null &&
            ( GameManager.Inst.localPlayer.Serial == data.actorInfo.serial || data.actorInfo.isLocal ) )
        {
            player = GameManager.Inst.localPlayer;
            player.IsLocal = true;
            player.gameObject.layer = Global.Layer.Player;
        }
        else
        {
            player = PoolManager.Inst.Get( data.actorInfo.prefab ) as Player;
            player.IsLocal = false;
            player.gameObject.layer = Global.Layer.Enemy;
        }

        player.Serial = data.actorInfo.serial;
        player.transform.SetPositionAndRotation( data.actorInfo.position.To(), data.actorInfo.rotation.To() );
        player.Nickname = data.nickname;
    }

    private void AckSpawnBullet( Packet _packet )
    {
        var data = Global.FromJson<BULLET_INFO>( _packet );
        Bullet bullet = PoolManager.Inst.Get( data.actorInfo.prefab ) as Bullet;
        bullet.IsLocal = data.actorInfo.isLocal;
        bullet.Serial = data.actorInfo.serial;
        bullet.Init( data.owner, data.actorInfo.position.To(), data.actorInfo.rotation.To() );
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Enemy enemy = PoolManager.Inst.Get( data.prefab ) as Enemy;
        enemy.Serial = data.serial;
        enemy.Initialize( new Vector3( data.position.x, data.position.y, data.position.z ) );
    }

    private void AckRemoveActor( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.Release();
    }

    private void AckSynkMovement( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.SetMovement( data.position.To(), data.rotation.To(), data.velocity.To() );
    }

    private void AckSynkReload( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.Weapon.reloadDelay.SetMax();
    }
    
    private void AckHitActor( Packet _packet )
    {
        var data = Global.FromJson<HIT_INFO>( _packet );
        Bullet bullet = GameManager.Inst.GetActor( data.bullet ) as Bullet;
        Character attacker = GameManager.Inst.GetActor( data.attacker ) as Character;
        Character defender = GameManager.Inst.GetActor( data.defender ) as Character;
        if ( bullet == null || attacker == null || defender == null )
        {
            Debug.LogWarning( $"Actor is null. {bullet}, {attacker}, {defender}" );
            return;
        }

        defender.OnHit( attacker, bullet );

        if ( data.needRelease )
        {
            bullet.Release();
        }
    }
    #endregion
}

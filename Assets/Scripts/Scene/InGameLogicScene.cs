using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class InGameLogicScene : SceneBase
{
    [SerializeField]
    private Transform spawnTransform;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_Logic;
        if ( spawnTransform == null )
        {
            spawnTransform = transform;
        }

        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK,        AckSpawnEnemy );
        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK,       AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_BULLET_ACK,       AckSpawnBullet );
        ProtocolSystem.Inst.Regist( REMOVE_ACTOR_ACK,       AckRemoveActor );
        ProtocolSystem.Inst.Regist( SYNC_MOVEMENT_ACK,      AckSyncMovement );
        ProtocolSystem.Inst.Regist( SYNC_RELOAD_ACK,        AckSyncReload );
        ProtocolSystem.Inst.Regist( SYNC_LOOK_ANGLE_ACK,    AckSyncLookAngle );
        ProtocolSystem.Inst.Regist( SYNC_DODGE_ACTION_ACK,  AckSyncDodgeAction );
        ProtocolSystem.Inst.Regist( SYNC_SWAP_WEAPON_ACK,   AckSyncSwapWeapon );
        ProtocolSystem.Inst.Regist( HIT_ACTOR_ACK,          AckHitActor );
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
        GameManager.LocalPlayer = player;
    }

    #region Req Protocols

    private void ReqInGameLoadData()
    {
        Player playerPrefab = GameManager.Inst.GetPlayerPrefab();

        // 접속시 생성할 Player 정보
        PLAYER_INFO protocol;
        protocol.actorInfo.isLocal = true;
        protocol.actorInfo.prefab = GameManager.Inst.GetPrefabIndex( playerPrefab );
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.pos = new VECTOR2( spawnTransform.position + new Vector3( Random.Range( -5f, 5f ), Random.Range( -5f, 5f ), 0f ) );
        protocol.actorInfo.vel = new VECTOR2( Vector2.zero );
        protocol.actorInfo.hp = playerPrefab.data.maxHp;
        protocol.nickname = string.Empty;
        protocol.isDead = false;
        protocol.angle = 0f;
        protocol.weapon = 1;
        protocol.kill = 0;
        protocol.death = 0;

        Network.Inst.Send( INGAME_LOAD_DATA_REQ, protocol );
    }
    #endregion

    #region Ack Protocols
    private void AckSpawnPlayer( Packet _packet )
    {
        var data = Global.FromJson<PLAYER_INFO>( _packet );
        Player player = null;
        if ( GameManager.LocalPlayer != null &&
            ( GameManager.LocalPlayer.Serial == data.actorInfo.serial || data.actorInfo.isLocal ) )
        {
            player = GameManager.LocalPlayer;
            player.IsLocal = true;
            player.gameObject.layer = Global.Layer.Player;
        }
        else
        {
            // 리스폰일시 기존 Player를 재사용한다
            player = GameManager.Players.Find( ( player ) => player.Serial == data.actorInfo.serial );
            if ( player == null )
            {
                player = PoolManager.Inst.Get( data.actorInfo.prefab ) as Player;
            }
            player.IsLocal = false;
            player.gameObject.layer = Global.Layer.Enemy;
        }

        player.gameObject.SetActive( true );
        player.Serial = data.actorInfo.serial;
        player.transform.position = data.actorInfo.pos.To();
        player.Rigid2D.velocity = data.actorInfo.vel.To();
        player.Hp.Current = data.actorInfo.hp;
        player.Nickname = data.nickname;
        player.IsDead = data.isDead;
        player.KillScore = data.kill;
        player.DeathScore = data.death;
        player.SwapWeapon( data.weapon );
        player.ApplyLookAngle( data.angle );
        player.gameObject.SetActive( !player.IsDead );

        if ( !GameManager.Players.Contains( player )  )
        {
            GameManager.Inst.AddPlayer( player );
        }
    }

    private void AckSpawnBullet( Packet _packet )
    {
        var data = Global.FromJson<BULLET_SHOT_INFO>( _packet );
        for ( int i = 0; i < data.bullets.Count; ++i )
        {
            Bullet bullet = PoolManager.Inst.Get( data.prefab ) as Bullet;
            bullet?.Fire( data, data.bullets[i] );
        }

        Character owner = GameManager.Inst.GetActor( data.owner ) as Character;
        if ( !ReferenceEquals( owner, null ) && !owner.IsLocal
            && !ReferenceEquals( owner.EquipWeapon, null ) )
        {
            owner.EquipWeapon.LookAngle( true, data.look );
        }
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Enemy enemy = PoolManager.Inst.Get( data.prefab ) as Enemy;
        enemy.Serial = data.serial;
        enemy.Initialize( new Vector2( data.pos.x, data.pos.y ) );
    }

    private void AckRemoveActor( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.Release();
    }

    private void AckSyncMovement( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.SetMovement( data.pos.To(), data.vel.To() );
    }

    private void AckSyncReload( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.EquipWeapon.stat.reloadDelay.SetMax();
    }

    private void AckSyncLookAngle( Packet _packet )
    {
        var data = Global.FromJson<LOOK_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            return;
        }
        player.ApplyLookAngle( data.angle );
    }

    private void AckSyncDodgeAction( Packet _packet )
    {
        var data = Global.FromJson<DODGE_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.transform.position = data.pos.To();
        player.AckDodgeAction( data.useCollision, data.dir.To(), data.dur );
    }

    private void AckSyncSwapWeapon( Packet _packet )
    {
        var data = Global.FromJson<INDEX_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.SwapWeapon( data.index );
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

        bullet.HitTarget( attacker, defender );

        if ( data.needRelease )
        {
            bullet.Release();
        }
    }
    #endregion
}

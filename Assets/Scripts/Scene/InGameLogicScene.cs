using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class InGameLogicScene : SceneBase
{
    [SerializeField]
    private List<Transform> spawnTransforms;
    [SerializeField]
    private Transform sceneActors;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_Logic;
        if ( spawnTransforms.Count == 0 )
        {
            spawnTransforms.Add( transform );
        }

        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK,        AckSpawnEnemy );
        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK,       AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_BULLET_ACK,       AckSpawnBullet );
        ProtocolSystem.Inst.Regist( REMOVE_ACTORS_ACK,      AckRemoveActors );
        ProtocolSystem.Inst.Regist( INIT_SCENE_ACTORS_ACK,  AckInitSceneActors );
        ProtocolSystem.Inst.Regist( SYNC_MOVEMENT_ACK,      AckSyncMovement );
        ProtocolSystem.Inst.Regist( SYNC_RELOAD_ACK,        AckSyncReload );
        ProtocolSystem.Inst.Regist( SYNC_LOOK_ANGLE_ACK,    AckSyncLookAngle );
        ProtocolSystem.Inst.Regist( SYNC_DODGE_ACTION_ACK,  AckSyncDodgeAction );
        ProtocolSystem.Inst.Regist( SYNC_SWAP_WEAPON_ACK,   AckSyncSwapWeapon );
        ProtocolSystem.Inst.Regist( HIT_ACTORS_ACK,         AckHitActors );
        ProtocolSystem.Inst.Regist( GAME_OVER_ACK,          AckGameOver );
        ProtocolSystem.Inst.Regist( UPDATE_RESULT_INFO_ACK, AckUpdateResultInfo );
    }

    protected override void Start()
    {
        base.Start();
        InitLocalPlayer();

        // is Host
        if ( GameManager.StageInfo.Value.personnel.current == 1 )
        {
            ReqInitSceneActors();
        }
        ReqInGameLoadData();
    }
    #endregion

    public Vector3 GetSpawnPosition()
    {
        int index = UnityEngine.Random.Range( 0, spawnTransforms.Count );
        return spawnTransforms[index].position;
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
        protocol.actorInfo.pos = new VECTOR2( GetSpawnPosition() );
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

    private void ReqInitSceneActors()
    {
        ACTORS_INFO protocol;
        protocol.actors = new List<ACTOR_INFO>();

        // Scene에 배치된 Actor 등록 및 Serial을 요청한다
        Actor[] actors = sceneActors.GetComponentsInChildren<Actor>();
        foreach ( Actor actor in actors )
        {
            actor.IsLocal = false;

            ACTOR_INFO actorInfo;
            actorInfo.isLocal = true;
            // 다른 클라에서 찾기 위한 용도
            actorInfo.prefab = actor.MyHashCode; 
            actorInfo.serial = 0;
            actorInfo.pos = new VECTOR2( actor.transform.position );
            actorInfo.vel = new VECTOR2( actor.Rigid2D.velocity );
            actorInfo.hp = actor.Hp.Current;
            protocol.actors.Add( actorInfo );
        }

        Network.Inst.Send( INIT_SCENE_ACTORS_REQ, protocol );
    }
    #endregion

    #region Ack Protocols
    private void AckUpdateResultInfo( Packet _packet )
    {
        GameManager.UserInfo = Global.FromJson<USER_INFO>( _packet );
    }

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
        player.ResetExcludeLayers();
        player.gameObject.SetActive( !player.IsDead );
        if ( player.gameObject.activeSelf )
        {
            player.SetInvincibleTime( GameManager.Inst.data.respawnInvincibleTime );
        }

        if ( !GameManager.Players.Contains( player ) )
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
        if ( !ReferenceEquals( owner, null )
            && !ReferenceEquals( owner.EquipWeapon, null ) )
        {
            if ( !owner.IsLocal )
            {
                owner.EquipWeapon.LookAngle( true, data.look );
            }
            owner.EquipWeapon.InvokeOnFire();
        }
    }

    private void AckSpawnEnemy( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Enemy enemy = PoolManager.Inst.Get( data.prefab ) as Enemy;
        enemy.Serial = data.serial;
        enemy.Initialize( new Vector2( data.pos.x, data.pos.y ) );
    }

    private void AckRemoveActors( Packet _packet )
    {
        var data = Global.FromJson<SERIALS_INFO>( _packet );
        foreach ( uint serial in data.serials )
        {
            Actor actor = GameManager.Inst.GetActor( serial );
            actor?.Release();
        }
    }

    private void AckInitSceneActors( Packet _packet )
    {
        var data = Global.FromJson<ACTORS_INFO>( _packet );
        Dictionary<int/*HashCode*/, ACTOR_INFO> actorHashs = new Dictionary<int, ACTOR_INFO>();
        foreach ( ACTOR_INFO actorInfo in data.actors )
        {
            actorHashs.Add( actorInfo.prefab, actorInfo );
        }

        Actor[] actors = sceneActors.GetComponentsInChildren<Actor>();
        foreach ( Actor actor in actors )
        {
            ACTOR_INFO actorInfo;
            if ( !actorHashs.TryGetValue( actor.MyHashCode, out actorInfo ) )
            {
                actor.Release();
                continue;
            }

            actor.Serial = actorInfo.serial;
            actor.transform.position = actorInfo.pos.To();
            if ( !actor.gameObject.isStatic )
            {
                actor.Rigid2D.velocity = actorInfo.vel.To();
            }
            actor.Hp.Current = actorInfo.hp;
        }
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
        player.EquipWeapon.myStat.reloadDelay.SetMax();
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

    private void AckHitActors( Packet _packet )
    {
        var data = Global.FromJson<HITS_INFO>( _packet );
        foreach ( HIT_INFO hit in data.hits )
        {
            Bullet bullet = GameManager.Inst.GetActor( hit.bullet ) as Bullet;
            Actor defender = GameManager.Inst.GetActor( hit.defender );
            if ( bullet == null || defender == null )
            {
                Debug.LogWarning( $"Actor is null. {bullet}, {defender}" );
                return;
            }

            bullet.transform.position = hit.pos.To();
            bullet.HitTarget( defender );
            defender.SetHp( hit.hp, GameManager.Inst.GetActor( hit.attacker ), bullet );
        }
    }

    private void AckGameOver( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );

        Player winner = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( ReferenceEquals( winner, null ) )
        {
            Debug.LogWarning( $"Player is null. serial:{data.serial}" );
        }

        RESULT_INFO protocol;
        protocol.uid   = GameManager.LoginInfo.Value.uid;
        protocol.kill  = GameManager.LocalPlayer.KillScore;
        protocol.death = GameManager.LocalPlayer.DeathScore;
        Network.Inst.Send( new Packet( UPDATE_RESULT_INFO_REQ, protocol ) );

        GameManager.Inst.GameOver( winner );
    }
    #endregion
}

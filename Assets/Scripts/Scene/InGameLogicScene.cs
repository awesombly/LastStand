using Cinemachine;
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
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        IsLock = true;
        SceneType = SceneType.InGame_Logic;
        AudioManager.Inst.Play( BGM.InGame, 0f, .5f, 5f );

        if ( spawnTransforms.Count == 0 )
        {
            spawnTransforms.Add( transform );
        }

        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;

        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK,            AckSpawnActor );
        ProtocolSystem.Inst.Regist( RESPAWN_ACTOR_ACK,          AckRespawnActor );
        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK,           AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_BULLET_ACK,           AckSpawnBullet );
        ProtocolSystem.Inst.Regist( REMOVE_ACTORS_ACK,          AckRemoveActors );
        ProtocolSystem.Inst.Regist( INIT_SCENE_ACTORS_ACK,      AckInitSceneActors );

        ProtocolSystem.Inst.Regist( SYNC_MOVEMENT_ACK,          AckSyncMovement );
        ProtocolSystem.Inst.Regist( SYNC_RELOAD_ACK,            AckSyncReload );
        ProtocolSystem.Inst.Regist( SYNC_LOOK_ANGLE_ACK,        AckSyncLookAngle );
        ProtocolSystem.Inst.Regist( SYNC_DODGE_ACTION_ACK,      AckSyncDodgeAction );
        ProtocolSystem.Inst.Regist( SYNC_SWAP_WEAPON_ACK,       AckSyncSwapWeapon );
        ProtocolSystem.Inst.Regist( SYNC_INTERACTION_ACK,       AckSyncInteraction );
        ProtocolSystem.Inst.Regist( SYNC_EATABLE_TARGET_ACK,    AckSyncEatableTarget );
        ProtocolSystem.Inst.Regist( SYNC_EATABLE_EVENT_ACK,     AckSyncEatableEvent );
        ProtocolSystem.Inst.Regist( HIT_ACTORS_ACK,             AckHitActors );

        ProtocolSystem.Inst.Regist( GAME_OVER_ACK,              AckGameOver );
    }

    protected override void Start()
    {
        base.Start();

        // 처음 입장한 사람이면
        if ( GameManager.StageInfo is not null
            && GameManager.StageInfo.Value.personnel.current == 1 )
        {
            ReqInitSceneActors();
        }
        ReqInGameLoadData();
    }

    private void OnDestroy()
    {
        GameManager.OnChangeLocalPlayer -= OnChangeLocalPlayer;
    }
    #endregion

    public Vector3 GetSpawnPosition()
    {
        int index = UnityEngine.Random.Range( 0, spawnTransforms.Count );
        return spawnTransforms[index].position;
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _new is not null )
        {
            virtualCamera.Follow = _new.transform;
        }
    }

    #region Req Protocols
    private void ReqInitSceneActors()
    {
        ACTORS_INFO protocol;
        protocol.actors = new List<ACTOR_INFO>();

        // Scene에 배치된 Actor 등록 및 Serial을 요청한다
        Actor[] actors = sceneActors.GetComponentsInChildren<Actor>();
        foreach ( Actor actor in actors )
        {
            actor.IsLocal = true;

            ACTOR_INFO actorInfo;
            actorInfo.isLocal = true;
            actorInfo.prefab = 0;
            actorInfo.hash = actor.MyHashCode;
            actorInfo.serial = 0;
            actorInfo.pos = new VECTOR2( actor.transform.position );
            actorInfo.vel = new VECTOR2( actor.Rigid2D.velocity );
            actorInfo.hp = actor.Hp.Current;
            actorInfo.inter = 0f;
            protocol.actors.Add( actorInfo );
            
            // 너무 많으면 패킷 사이즈를 초과해서 나눠보낸다
            if ( protocol.actors.Count >= 20 )
            {
                Network.Inst.Send( INIT_SCENE_ACTORS_REQ, protocol );
                protocol.actors.Clear();
            }
        }

        if ( protocol.actors.Count > 0 )
        {
            Network.Inst.Send( INIT_SCENE_ACTORS_REQ, protocol );
        }
    }

    private void ReqInGameLoadData()
    {
        Network.Inst.Send( INGAME_LOAD_DATA_REQ );
    }
    #endregion

    #region Ack Protocols
    #region Spawn, Remove
    private void AckSpawnActor( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Actor actor = PoolManager.Inst.Get( data.prefab ) as Actor;
        if ( actor is null )
        {
            Debug.LogWarning( $"Actor is null. prefab:{data.prefab}" );
            return;
        }
        actor.Serial = data.serial;
        actor.SetMovement( data.pos.To(), data.vel.To() );
        actor.SetHp( data.hp, null, null, SyncType.FromServer );
    }

    private void AckRespawnActor( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        DestroyableActor actor = GameManager.Inst.GetActor( data.serial ) as DestroyableActor;
        if ( actor is null )
        {
            Debug.LogWarning( $"Actor is null. serial:{data.serial}" );
            return;
        }
        actor.RespawnActor();
    }

    private void AckSpawnPlayer( Packet _packet )
    {
        var data = Global.FromJson<PLAYER_INFO>( _packet );
        Player player = null;
        if ( data.actorInfo.isLocal )
        {
            if ( GameManager.LocalPlayer is not null
                && GameManager.LocalPlayer.Serial == data.actorInfo.serial )
            {
                // 리스폰시
                player = GameManager.LocalPlayer;
            }
            else
            {
                player = PoolManager.Inst.Get( data.actorInfo.prefab ) as Player;
            }
            player.IsLocal = true;
            player.gameObject.layer = Global.Layer.Player;
            GameManager.LocalPlayer = player;
        }
        else
        {
            // 리스폰일시 기존 Player를 재사용한다
            player = GameManager.Inst.FindPlayer( data.actorInfo.serial );
            if ( player is null )
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
        player.PlayerType = data.type;
        player.SwapWeapon( data.weapon );
        player.ApplyLookAngle( data.angle );
        player.UnactionableCount = 0;
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

        player?.OnRespawn();
    }

    private void AckSpawnBullet( Packet _packet )
    {
        var data = Global.FromJson<BULLET_SHOT_INFO>( _packet );
        for ( int i = 0; i < data.bullets.Count; ++i )
        {
            Bullet bullet = PoolManager.Inst.Get( data.prefab ) as Bullet;
            bullet?.Fire( data, data.bullets[i] );
        }

        if ( data.isExplode )
        {
            return;
        }

        Character owner = GameManager.Inst.GetActor( data.owner ) as Character;
        if ( owner is not null && owner.EquipWeapon is not null )
        {
            if ( !owner.IsLocal )
            {
                owner.EquipWeapon.LookAngle( true, data.look );
            }
            owner.EquipWeapon.InvokeOnFire();
        }
    }

    private void AckRemoveActors( Packet _packet )
    {
        var data = Global.FromJson<SERIALS_INFO>( _packet );
        foreach ( uint serial in data.serials )
        {
            Player player = GameManager.Inst.FindPlayer( serial );
            if ( player is not null )
            {
                player.Release();
                continue;
            }

            Actor actor = GameManager.Inst.GetActor( serial );
            actor?.Release();
        }
    }
    #endregion

    #region Sync
    private void AckSyncMovement( Packet _packet )
    {
        var data = Global.FromJson<MOVEMENT_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.SetMovement( data.pos.To(), data.vel.To() );
    }

    private void AckSyncReload( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player is null )
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
        if ( player is null )
        {
            return;
        }
        player.ApplyLookAngle( data.angle );
    }

    private void AckSyncDodgeAction( Packet _packet )
    {
        var data = Global.FromJson<DODGE_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player is null )
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
        if ( player is null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.SwapWeapon( data.index );
    }

    private void AckSyncInteraction( Packet _packet )
    {
        var data = Global.FromJson<INTERACTION_INFO>( _packet );
        InteractableActor target = GameManager.Inst.GetActor( data.target ) as InteractableActor;
        if ( target is null )
        {
            Debug.LogWarning( "Target is null. serial:" + data.target );
            return;
        }
        Player player = GameManager.Inst.FindPlayer( data.serial );
        target.transform.position = data.pos.To();
        target.InteractionAction( data.angle, player );
    }

    private void AckSyncEatableTarget( Packet _packet )
    {
        var data = Global.FromJson<INTERACTION_INFO>( _packet );
        EatableActor eatable = GameManager.Inst.GetActor( data.serial ) as EatableActor;
        if ( eatable is null )
        {
            Debug.LogWarning( "Target is null. serial:" + data.serial );
            return;
        }
        Player player = GameManager.Inst.FindPlayer( data.target );
        eatable.SetTarget( player, data.pos.To() );
    }

    private void AckSyncEatableEvent( Packet _packet )
    {
        var data = Global.FromJson<INTERACTION_INFO>( _packet );
        EatableActor eatable = GameManager.Inst.GetActor( data.serial ) as EatableActor;
        if ( eatable is null )
        {
            Debug.LogWarning( "Target is null. serial:" + data.serial );
            return;
        }
        Player player = GameManager.Inst.FindPlayer( data.target );
        eatable.InvokeEatEvent( player, data.angle );
    }

    private void AckHitActors( Packet _packet )
    {
        var data = Global.FromJson<HITS_INFO>( _packet );
        foreach ( HIT_INFO hit in data.hits )
        {
            Actor defender = GameManager.Inst.GetActor( hit.defender );
            if ( defender is null )
            {
                if ( hit.needRelease )
                {
                    Bullet bul = GameManager.Inst.GetActor( hit.hiter ) as Bullet;
                    bul?.Release();
                }
                continue;
            }

            // Bullet일 경우
            Bullet bullet = GameManager.Inst.GetActor( hit.hiter ) as Bullet;
            if ( bullet is not null )
            {
                if ( bullet.IsLocal )
                {
                    bullet.HitTarget( defender, SyncType.LocalEcho, hit.hp );
                }
                else
                {
                    bullet.transform.position = hit.pos.To();
                    bullet.HitTarget( defender, SyncType.FromServer, hit.hp );
                }

                if ( hit.needRelease )
                {
                    bullet.Release();
                }
                continue;
            }

            // 구르기에 맞은 경우
            Player attacker = GameManager.Inst.GetActor( hit.attacker ) as Player;
            if ( attacker is not null )
            {
                SyncType syncType = attacker.IsLocal ? SyncType.LocalEcho : SyncType.FromServer;
                attacker.DodgeAttack.HitTarget( defender, syncType, hit.hp );
            }
        }
    }
    #endregion

    private void AckInitSceneActors( Packet _packet )
    {
        var data = Global.FromJson<ACTORS_INFO>( _packet );
        Dictionary<int/*HashCode*/, ACTOR_INFO> actorHashs = new Dictionary<int, ACTOR_INFO>();
        foreach ( ACTOR_INFO actorInfo in data.actors )
        {
            if ( !actorHashs.TryAdd( actorInfo.hash, actorInfo ) )
            {
                Debug.LogWarning( $"Invalid Hash. hash:{actorInfo.hash}, serial:{actorInfo.serial}" );
            }
        }

        Actor[] actors = sceneActors.GetComponentsInChildren<Actor>();
        foreach ( Actor actor in actors )
        {
            ACTOR_INFO actorInfo;
            if ( !actorHashs.TryGetValue( actor.MyHashCode, out actorInfo ) )
            {
                continue;
            }

            actor.Serial = actorInfo.serial;
            actor.transform.position = actorInfo.pos.To();
            if ( !actor.gameObject.isStatic )
            {
                actor.Rigid2D.velocity = actorInfo.vel.To();
            }
            actor.SetHp( actorInfo.hp, null, null, SyncType.FromServer );
            if ( actor is InteractableActor interactable && !actor.Hp.IsZero )
            {
                interactable.InteractionAction( actorInfo.inter, null, true );
            }
        }
    }

    private void AckGameOver( Packet _packet )
    {
        var data = Global.FromJson<SERIAL_INFO>( _packet );

        Player winner = GameManager.Inst.FindPlayer( data.serial );
        if ( winner is null )
        {
            Debug.LogWarning( $"Player is null. serial:{data.serial}" );
        }

        GameManager.Inst.GameOver( winner );
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

public class InGameScene : SceneBase
{
    [SerializeField]
    private Transform spawnTransform;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame;
        if ( spawnTransform == null )
        {
            spawnTransform = transform;
        }

        ProtocolSystem.Inst.Regist( SPAWN_ACTOR_ACK,        AckSpawnEnemy );
        ProtocolSystem.Inst.Regist( SPAWN_PLAYER_ACK,       AckSpawnPlayer );
        ProtocolSystem.Inst.Regist( SPAWN_BULLET_ACK,       AckSpawnBullet );
        ProtocolSystem.Inst.Regist( REMOVE_ACTOR_ACK,       AckRemoveActor );
        ProtocolSystem.Inst.Regist( SYNK_MOVEMENT_ACK,      AckSynkMovement );
        ProtocolSystem.Inst.Regist( SYNK_RELOAD_ACK,        AckSynkReload );
        ProtocolSystem.Inst.Regist( SYNK_LOOK_ANGLE_ACK,    AckSynkLookAngle );
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
        // 立加矫 积己且 Player 沥焊
        PLAYER_INFO protocol;
        protocol.actorInfo.isLocal = true;
        protocol.actorInfo.prefab = GameManager.Inst.GetPlayerPrefabIndex();
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.pos = new VECTOR2( spawnTransform.position + new Vector3( Random.Range( -5f, 5f ), Random.Range( -5f, 5f ), 0f ) );
        protocol.actorInfo.vel = new VECTOR2( Vector2.zero );
        protocol.nickname = string.Empty;

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
            player = PoolManager.Inst.Get( data.actorInfo.prefab ) as Player;
            player.IsLocal = false;
            player.gameObject.layer = Global.Layer.Enemy;
        }

        player.Serial = data.actorInfo.serial;
        player.transform.position = data.actorInfo.pos.To();
        player.Nickname = data.nickname;
    }

    private void AckSpawnBullet( Packet _packet )
    {
        var data = Global.FromJson<BULLET_INFO>( _packet );
        Bullet bullet = PoolManager.Inst.Get( data.prefab ) as Bullet;
        bullet.IsLocal = data.isLocal;
        bullet.Serial = data.serial;
        bullet.Init( data.owner, data.pos.To(), data.angle );

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

    private void AckSynkMovement( Packet _packet )
    {
        var data = Global.FromJson<ACTOR_INFO>( _packet );
        Actor actor = GameManager.Inst.GetActor( data.serial );
        actor?.SetMovement( data.pos.To(), data.vel.To() );
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
        player.EquipWeapon.reloadDelay.SetMax();
    }

    private void AckSynkLookAngle( Packet _packet )
    {
        var data = Global.FromJson<LOOK_INFO>( _packet );
        Player player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( player == null )
        {
            Debug.LogWarning( "Player is null. serial:" + data.serial );
            return;
        }
        player.LookAngle( data.angle );
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

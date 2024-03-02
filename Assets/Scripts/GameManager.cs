using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    public static Vector2 MouseWorldPos { get; private set; }
    public static Vector2 MouseDirection { get; private set; }
    public static float LookAngle { get; private set; }

    private static Player localPlayer;
    public static Player LocalPlayer
    {
        get => localPlayer;
        set
        {
            if ( localPlayer == value )
            {
                return;
            }

            Player oldPlayer = localPlayer;
            localPlayer = value;
            OnChangeLocalPlayer?.Invoke( oldPlayer, localPlayer );
        }
    }
    public static event Action<Player/*old*/, Player/*new*/> OnChangeLocalPlayer;

    public static List<Player> Players = new List<Player>();
    public static event Action OnChangePlayers;
    public static event Action<Player/*winner*/> OnGameOver;
    public static event Action<Player/* Dead Player */> OnDead;

    public static STAGE_INFO? StageInfo { get; set; }
    public static LOGIN_INFO? LoginInfo { get; set; }
    public static USER_INFO?  UserInfo  { get; set; }

    public GameManagerSO data;
    private Dictionary<SceneType, SceneBase> activeScenes = new Dictionary<SceneType, SceneBase>();
    private Dictionary<uint/*Serial*/, Actor> actors = new Dictionary<uint, Actor>();

    private SERIALS_INFO removeActorsToSend = new SERIALS_INFO();
    private HITS_INFO hitsInfoToSend = new HITS_INFO();

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        InputSystem.EnableDevice( Mouse.current );
        InputSystem.EnableDevice( Keyboard.current );

        removeActorsToSend.serials = new List<uint>();
        hitsInfoToSend.hits = new List<HIT_INFO>();

        SceneBase.OnBeforeSceneLoad += Clear;
        SceneBase.OnAfterSceneLoad += UpdateActiveScene;
    }

    private void Update()
    {
        // Update Parameters
        MouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        if ( !ReferenceEquals( LocalPlayer, null ) )
        {
            LookAngle = Global.GetAngle( LocalPlayer.transform.position, MouseWorldPos );
            MouseDirection = ( MouseWorldPos - ( Vector2 )LocalPlayer.transform.position ).normalized;
        }

        // Update Players
        foreach ( Player player in Players )
        {
            if ( !ReferenceEquals( player, null ) )
            {
                player.UpdateUI();
            }
        }
    }

    private void FixedUpdate()
    {
        // Send Remove Actor
        if ( removeActorsToSend.serials.Count >= 1 )
        {
            Network.Inst.Send( PacketType.REMOVE_ACTORS_REQ, removeActorsToSend );
            removeActorsToSend.serials.Clear();
        }

        // Send Hit Actor
        if ( hitsInfoToSend.hits.Count >= 1 )
        {
            Network.Inst.Send( PacketType.HIT_ACTORS_REQ, hitsInfoToSend );
            hitsInfoToSend.hits.Clear();
        }
    }
    #endregion

    #region Player
    public void PlayerDead( Player _dead, Actor _attacker, Bullet _bullet )
    {
        // 플레이어 사망시, 같은 객체를 재사용한다.
        _dead.gameObject.SetActive( false );
        OnDead?.Invoke( _dead );

        if ( _dead.IsLocal )
        {
            StartCoroutine( PlayerRespawn( _dead ) );
        }
    }

    private IEnumerator PlayerRespawn( Player _player )
    {
        _player.PlayerUI.respawnDelay.Max = data.respawnDelay;
        _player.PlayerUI.respawnDelay.SetMax();
        while ( !_player.PlayerUI.respawnDelay.IsZero )
        {
            _player.PlayerUI.respawnDelay.Current -= Time.deltaTime;
            yield return null;
        }

        if ( _player == null )
        {
            yield break;
        }

        var scene = GetActiveScene( SceneType.InGame_Logic ) as InGameLogicScene;
        if ( ReferenceEquals( scene, null ) )
        {
            Debug.LogError( "InGameLogicScene is null." );
            yield break;
        }

        PLAYER_INFO protocol;
        protocol.actorInfo.isLocal = true;
        protocol.actorInfo.prefab = _player.PrefabIndex;
        protocol.actorInfo.serial = _player.Serial;
        protocol.actorInfo.pos = new VECTOR2( scene.GetSpawnPosition() );
        protocol.actorInfo.vel = new VECTOR2( Vector2.zero );
        protocol.actorInfo.hp = _player.Hp.Max;
        protocol.nickname = _player.Nickname;
        protocol.isDead = false;
        protocol.angle = _player.LookAngle;
        protocol.weapon = 1;
        protocol.kill = _player.KillScore;
        protocol.death = _player.DeathScore;
        Network.Inst.Send( PacketType.SPAWN_PLAYER_REQ, protocol );
    }

    public void AddPlayer( Player _player )
    {
        if ( _player == null || Players.Contains( _player ) )
        {
            Debug.LogWarning( $"Invalid Player. {_player}" );
            return;
        }

        Players.Add( _player );
        OnChangePlayers?.Invoke();
    }

    public void RemovePlayer( Player _player )
    {
        if ( _player == null || !Players.Contains( _player ) )
        {
            Debug.LogWarning( $"Invalid Player. {_player}" );
            return;
        }

        Players.Remove( _player );
        OnChangePlayers?.Invoke();
    }
    #endregion

    #region Actor
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
    #endregion

    #region Prefab
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
    #endregion

    #region Scene
    public SceneBase GetActiveScene( SceneType _sceneType )
    {
        if ( !activeScenes.ContainsKey( _sceneType ) )
        {
            Debug.LogWarning( $"Not active SceneType :{_sceneType}" );
            return null;
        }

        return activeScenes[ _sceneType ];
    }

    private void UpdateActiveScene()
    {
        activeScenes.Clear();

        SceneBase[] scenes = FindObjectsByType<SceneBase>( FindObjectsSortMode.None );
        foreach ( SceneBase scene in scenes)
        {
            activeScenes.Add( scene.SceneType, scene );
        }
    }
    #endregion

    #region Req Protocol
    public void PushRemoveActorToSend( uint _serial )
    {
        removeActorsToSend.serials.Add( _serial );
    }

    public void PushHitInfoToSend( HIT_INFO hit )
    {
        hitsInfoToSend.hits.Add( hit );
        // 양이 많으면 바로 Send. (패킷 4096 바이트 기준, 34개 정도면 사이즈를 벗어난다)
        if ( hitsInfoToSend.hits.Count >= 20 )
        {
            Network.Inst.Send( PacketType.HIT_ACTORS_REQ, hitsInfoToSend );
            hitsInfoToSend.hits.Clear();
        }
    }
    #endregion

    public void GameOver( Player _winner )
    {
        OnGameOver?.Invoke( _winner );
    }

    private void Clear()
    {
        LocalPlayer = null;
        Players.Clear();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Player : Character
{
    #region UI
    private string nickname;
    public string Nickname 
    {
        get => nickname;
        set
        {
            nickname = value;
            nicknameUI?.SetText( nickname );
        }
    }
    private int killScore;
    public int KillScore 
    {
        get => killScore;
        set
        {
            killScore = value;
            board?.UpdateKillCount( killScore );
        }
    }
    private int deathScore;
    public int DeathScore
    {
        get => deathScore;
        set
        {
            deathScore = value;
            board?.UpdateDeathCount( deathScore );
        }
    }

    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    [SerializeField]
    private UnityEngine.UI.Slider healthBar;
    [SerializeField]
    private UnityEngine.UI.Slider healthLerpBar;
    [SerializeField]
    private PlayerChatMessage chatMessage;
    private PlayerBoard board;
    #endregion

    public Vector2 Direction { get; set; }
    private List<Weapon> Weapons { get; set; } = null;

    #region Components
    public PlayerUI PlayerUI { get; private set; }
    private PlayerInput playerInput;
    private PlayerAnimator playerAnimator;
    private ActionReceiver receiver;
    private PlayerMovement movement;
    #endregion

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        PlayerUI = GetComponent<PlayerUI>();
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<PlayerAnimator>();
        receiver = GetComponent<ActionReceiver>();
        movement = GetComponent<PlayerMovement>();
        Weapons = new List<Weapon>( GetComponentsInChildren<Weapon>( true ) );

        receiver.OnSwapWeaponEvent += SwapWeapon;
        receiver.OnInteractionEvent += Interaction;

        healthBar.value = Hp.Current / Hp.Max;
        healthLerpBar.value = healthBar.value;
        Hp.OnChangeCurrent += OnChangeHp;
    }
    
    protected override void Start()
    {
        base.Start();
    }
    #endregion

    public void InitBoardUI( PlayerBoard _board )
    {
        board = _board;
        board?.UpdateHealth( healthBar.value );
        board?.UpdateKillCount( KillScore );
        board?.UpdateDeathCount( DeathScore );
    }

    public void UpdateUI()
    {
        healthLerpBar.value = Mathf.Max( healthLerpBar.value - 0.1f * Time.deltaTime, healthBar.value );
        healthLerpBar.value = Mathf.Lerp( healthLerpBar.value, healthBar.value, 2f * Time.deltaTime );
        board?.UpdateHealthLerp( healthLerpBar.value );
    }

    public void ReceiveMessage( string _message )
    {
        chatMessage.gameObject.SetActive( true );
        chatMessage.UpdateMessage( _message );
    }

    public void SwapWeapon( int _index )
    {
        if ( _index <= 0 || _index >= Weapons.Count )
        {
            return;
        }

        if ( !ReferenceEquals( EquipWeapon, null )
            && !EquipWeapon.myStat.swapDelay.IsZero )
        {
            return;
        }

        EquipWeapon = Weapons[_index];

        if ( IsLocal )
        {
            INDEX_INFO protocol;
            protocol.serial = Serial;
            protocol.index = _index;
            Network.Inst.Send( PacketType.SYNC_SWAP_WEAPON_REQ, protocol );
        }
    }

    public void ResetExcludeLayers()
    {
        if ( gameObject.layer == Global.Layer.Player )
        {
            Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Enemy
                | Global.LayerFlag.EnemyAttack
                | Global.LayerFlag.Wall
                | Global.LayerFlag.Misc );
        }
        else
        {
            Rigid2D.excludeLayers = ~( int )( Global.LayerFlag.Player
                | Global.LayerFlag.Enemy
                | Global.LayerFlag.PlayerAttack
                | Global.LayerFlag.Wall
                | Global.LayerFlag.Misc );
        }
    }

    public void SetInvincibleTime( float _time )
    {
        StartCoroutine( UpdateInvincible( _time ) );
    }

    private IEnumerator UpdateInvincible( float _time )
    {
        int originLayer = gameObject.layer;
        SpriteRenderer spriter = gameObject.GetComponent<SpriteRenderer>();

        playerAnimator.SetReviveAction( true );
        ++UnactionableCount;
        gameObject.layer = Global.Layer.Invincible;
        while ( _time > 0f )
        {
            _time -= Time.deltaTime;
            float alpha = Mathf.Max( .4f, .4f + ( 2f - _time ) * .2f );
            spriter.color = new Color( 1f, 1f, 1f, alpha );
            yield return YieldCache.WaitForEndOfFrame;
        }
        spriter.color = Color.white;
        gameObject.layer = originLayer;
    }

    public override void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        movement.moveInfo.moveVector = _velocity;
    }

    public void AckDodgeAction( bool _useCollision, Vector2 _direction, float _duration )
    {
        movement.DodgeAction( _useCollision, _direction, _duration );
    }

    private void Interaction()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll( transform.position, 1f, ( int )Global.LayerFlag.Misc );
        foreach ( Collider2D col in colliders )
        {
            InteractableActor interactable = col.GetComponent<InteractableActor>();
            if ( ReferenceEquals( interactable, null ) )
            {
                continue;
            }

            interactable.Interaction( this );
        }
    }

    protected override void OnDead( Actor _attacker, Bullet _bullet )
    {
        if ( IsDead )
        {
            Debug.LogWarning( $"Already dead player. nick:{nickname}, name:{name}, attacker:{_attacker.name}" );
            return;
        }

        IsDead = true;
        ++DeathScore;

        Player attacker = _attacker as Player;
        if ( !ReferenceEquals( attacker, null ) )
        {
            ++attacker.KillScore;
        }

        playerAnimator.OnDead( _bullet.transform.up );
        GameManager.Inst.PlayerDead( this, _attacker, _bullet );
    }

    protected override void OnChangeLocal( bool _isLocal )
    {
        base.OnChangeLocal( _isLocal );
        playerInput.enabled = _isLocal;
    }

    private void OnChangeHp( float _old, float _new, float _max )
    {
        healthBar.value = _new / _max;
        if ( _old < _new )
        {
            healthLerpBar.value = healthBar.value;
        }

        board?.UpdateHealth( healthBar.value );
    }

    public override void Release()
    {
        EquipWeapon = null;
        GameManager.Inst.RemovePlayer( this );
        base.Release();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Player : Character
{
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
    public float KillScore { get; set; }
    public float DeathScore { get; set; }

    public Vector2 Direction { get; set; }

    private List<Weapon> Weapons { get; set; } = null;

    #region UI
    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    [SerializeField]
    private UnityEngine.UI.Slider healthBar;
    [SerializeField]
    private UnityEngine.UI.Slider healthLerpBar;
    private PlayerBoard board;
    #endregion

    #region Components
    private PlayerInput playerInput;
    private ActionReceiver receiver;
    private PlayerMovement movement;
    #endregion

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        receiver = GetComponent<ActionReceiver>();
        movement = GetComponent<PlayerMovement>();
        Weapons = new List<Weapon>( GetComponentsInChildren<Weapon>( true ) );

        receiver.OnSwapWeaponEvent += SwapWeapon;

        healthBar.value = Hp.Current / Hp.Max;
        healthLerpBar.value = healthBar.value;
        Hp.OnChangeCurrent += OnChangeHp;
    }

    protected override void Start()
    {
        base.Start();
        OnDeadEvent += OnDead;
    }

    private void Update()
    {
        healthLerpBar.value = Mathf.Max( healthLerpBar.value - 0.1f * Time.deltaTime, healthBar.value );
        healthLerpBar.value = Global.Mathematics.Lerp( healthLerpBar.value, healthBar.value, 2f * Time.deltaTime );
        board?.UpdateHealthLerp( healthLerpBar.value );
    }
    #endregion

    public void InitBoardUI( PlayerBoard _board )
    {
        board = _board;
        board?.UpdateHealth( healthBar.value );
    }

    public void SwapWeapon( int _index )
    {
        if ( _index <= 0 || _index >= Weapons.Count )
        {
            return;
        }

        if ( !ReferenceEquals( EquipWeapon, null )
            && !EquipWeapon.stat.swapDelay.IsZero )
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

    public override void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        movement.moveInfo.moveVector = _velocity;
    }

    public void AckDodgeAction( bool _useCollision, Vector2 _direction, float _duration )
    {
        movement.DodgeAction( _useCollision, _direction, _duration );
    }

    private void OnDead( Character _dead, Character _attacker )
    {
        Debug.Log( "Player dead." );
        Hp.SetMax();
    }

    protected override void OnChangeLocal( bool _isLocal )
    {
        base.OnChangeLocal( _isLocal );
        playerInput.enabled = _isLocal;
    }

    private void OnChangeHp( float _old, float _new )
    {
        healthBar.value = Hp.Current / Hp.Max;
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

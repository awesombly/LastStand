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

    public Vector2 Direction { get; set; }

    private List<Weapon> Weapons { get; set; } = null;

    #region UI
    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    [SerializeField]
    private UnityEngine.UI.Slider healthBar;
    [SerializeField]
    private UnityEngine.UI.Slider healthLerpBar;
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
        SwapWeapon( 1 );

        receiver.OnSwapWeaponEvent += SwapWeapon;

        healthBar.value = Hp.Current / Hp.Max;
        healthLerpBar.value = healthBar.value;
        Hp.OnChangeCurrent += OnChangeHp;
    }

    private void Start()
    {
        OnDeadEvent += OnDead;
    }

    private void Update()
    {
        healthLerpBar.value = Mathf.Max( healthLerpBar.value - 0.1f * Time.deltaTime, healthBar.value );
        healthLerpBar.value = Global.Mathematics.Lerp( healthLerpBar.value, healthBar.value, 2f * Time.deltaTime );
    }
    #endregion

    private void SwapWeapon( int _index )
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
    }
}

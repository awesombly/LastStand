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
    private PlayerMovement movement;
    #endregion

    public event Action<Weapon/*old*/, Weapon/*new*/> OnChangeEquipWeapon;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();
        
        healthBar.value = Hp.Current / Hp.Max;
        healthLerpBar.value = healthBar.value;
        Hp.OnChangeCurrent += OnChangeHp;
    }

    private void Start()
    {
        OnDeadEvent += OnDead;
        OnChangeEquipWeapon?.Invoke( null, EquipWeapon );
    }

    private void Update()
    {
        healthLerpBar.value = Mathf.Max( healthLerpBar.value - 0.1f * Time.deltaTime, healthBar.value );
        healthLerpBar.value = Global.Mathematics.Lerp( healthLerpBar.value, healthBar.value, 2f * Time.deltaTime );
    }
    #endregion

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

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

    #region Movement
    public Vector2 Direction { get; private set; }
    private Vector2 moveVector;
    private Vector3 prevMoveVector;
    private Vector2 prevPosition;
    private float prevAngle;
    [SerializeField]
    private float allowSynkDistance;
    [SerializeField]
    private float allowSynkAngle;
    [SerializeField]
    private Global.StatusFloat allowSynkInterval;
    #endregion

    #region UI
    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    [SerializeField]
    private UnityEngine.UI.Slider healthBar;
    [SerializeField]
    private UnityEngine.UI.Slider healthLerpBar;
    #endregion
    #region Components
    private SpriteRenderer spriter;
    private Animator animator;
    private ActionReceiver receiver;
    private PlayerInput playerInput;
    #endregion

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        receiver = GetComponent<ActionReceiver>();
        playerInput = GetComponent<PlayerInput>();

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

        if ( !IsLocal )
        {
            return;
        }

        allowSynkInterval.Current -= Time.deltaTime;
        UpdateLookAngle();
    }

    private void FixedUpdate()
    {
        if ( IsLocal )
        {
            moveVector = receiver.InputVector * data.moveSpeed;
            ReqSynkMovement();
        }

        Rigid2D.MovePosition( Rigid2D.position + moveVector * Time.fixedDeltaTime );

        Direction = ( Rigid2D.position - prevPosition ).normalized;
        prevPosition = Rigid2D.position;
        prevMoveVector = moveVector;
    }

    private void LateUpdate()
    {
        animator.SetFloat( "MoveSpeed", moveVector.sqrMagnitude );
    }
    #endregion

    private void ReqSynkMovement()
    {
        float velocityInterval = Vector2.Distance( moveVector, prevMoveVector );
        if ( velocityInterval >= allowSynkDistance )
        {
            ACTOR_INFO protocol;
            protocol.isLocal = false;
            protocol.prefab = 0;
            protocol.serial = Serial;
            protocol.position = new VECTOR3( Rigid2D.position );
            protocol.rotation = new QUATERNION( transform.rotation );
            protocol.velocity = new VECTOR3( moveVector );
            Network.Inst.Send( PacketType.SYNK_MOVEMENT_REQ, protocol );
        }
    }

    private void UpdateLookAngle()
    {
        Vector3 dir = ( GameManager.MouseWorldPos - transform.position ).normalized;
        float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;

        bool prevFlipX = IsFlipX;
        LookAngle( angle );

        #region ReqProtocol
        if ( allowSynkInterval.IsZero &&
            ( IsFlipX != prevFlipX || Mathf.Abs( angle - prevAngle ) >= allowSynkAngle ) )
        {
            allowSynkInterval.SetMax();

            LOOK_INFO protocol;
            protocol.serial = Serial;
            protocol.angle = angle;
            Network.Inst.Send( PacketType.SYNK_LOOK_REQ, protocol );
        }
        #endregion

        prevAngle = angle;
    }

    public override void SetMovement( Vector3 _position, Quaternion _rotation, Vector3 _velocity )
    {
        transform.position = _position;
        transform.rotation = _rotation;
        moveVector = _velocity;
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

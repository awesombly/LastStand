using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


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

    public Vector2 Direction { get; private set; }
    private Vector2 moveVector;
    private Vector3 prevMoveVector;
    private Vector2 prevPosition;
    [SerializeField]
    private float allowSynkInterval;

    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    private SpriteRenderer spriter;
    private Animator animator;
    private ActionReceiver receiver;
    private PlayerInput playerInput;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        receiver = GetComponent<ActionReceiver>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        OnDeadEvent += OnDead;
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

        if ( moveVector.x != 0f )
        {
            spriter.flipX = moveVector.x < 0f;
        }
    }
    #endregion

    private void ReqSynkMovement()
    {
        float velocityInterval = Vector2.Distance( moveVector, prevMoveVector );
        if ( velocityInterval >= allowSynkInterval )
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

    public override void SetMovement( Vector3 _position, Quaternion _rotation, Vector3 _moveVector )
    {
        Rigid2D.MovePosition( _position );
        transform.rotation = _rotation;
        moveVector = _moveVector;
    }

    private void OnDead( Character _dead, Character _attacker )
    {
        Debug.Log( "Player dead." );
        Hp = data.maxHp;
    }

    protected override void OnChangeLocal( bool _isLocal )
    {
        base.OnChangeLocal( _isLocal );
        playerInput.enabled = _isLocal;
    }
}

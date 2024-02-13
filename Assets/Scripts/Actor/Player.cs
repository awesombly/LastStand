using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : Character
{
    public Vector2 Direction { get; private set; }
    private Vector2 prevPosition;
    private Vector3 prevVelocity;
    [SerializeField]
    private float allowSynkInterval;

    public Rigidbody2D Rigid2D { get; private set; }
    private SpriteRenderer spriter;
    private Animator animator;
    private ActionReceiver receiver;
    private PlayerInput playerInput;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        Rigid2D = GetComponent<Rigidbody2D>();
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
        Rigid2D.velocity = receiver.InputVector * data.moveSpeed;

        Direction = ( Rigid2D.position - prevPosition ).normalized;

        if ( IsLocal )
        {
            ReqSynkMovement();
        }

        prevPosition = Rigid2D.position;
        prevVelocity = Rigid2D.velocity;
    }

    private void LateUpdate()
    {
        Vector2 inputVector = receiver.InputVector;

        animator.SetFloat( "MoveSpeed", inputVector.sqrMagnitude );

        if ( inputVector.x != 0f )
        {
            spriter.flipX = inputVector.x < 0f;
        }
    }
    #endregion

    private void ReqSynkMovement()
    {
        float velocityInterval = Vector2.Distance( Rigid2D.velocity, prevVelocity );
        if ( velocityInterval >= allowSynkInterval )
        {
            ACTOR_INFO protocol;
            protocol.isLocal = false;
            protocol.prefab = 0;
            protocol.serial = 0;
            protocol.position = new VECTOR3( Rigid2D.position );
            protocol.rotation = new QUATERNION( transform.rotation );
            protocol.velocity = new VECTOR3( Rigid2D.velocity );
            Network.Inst.Send( PacketType.SYNK_MOVEMENT_REQ, protocol );
        }
    }

    public override void SetMovement( Vector3 _position, Quaternion _rotation, Vector3 _velocity )
    {
        Rigid2D.MovePosition( _position );
        transform.rotation = _rotation;
        Rigid2D.velocity = _velocity;
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

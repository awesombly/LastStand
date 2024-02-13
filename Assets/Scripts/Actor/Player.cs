using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : Character
{
    public Vector2 Direction { get; private set; }
    private Vector2 prevPosition;

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
        Vector2 delta = receiver.InputVector * data.moveSpeed * Time.fixedDeltaTime;
        Rigid2D.MovePosition( Rigid2D.position + delta );

        Direction = ( Rigid2D.position - prevPosition ).normalized;
        prevPosition = Rigid2D.position;
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

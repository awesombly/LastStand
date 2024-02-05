using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public Action<InputValue> OnMoveEvent;
    public Action<InputValue> OnAttackEvent;

    public Vector2 Direction { get; private set; }

    [SerializeField]
    private float moveSpeed;

    private Vector2 inputVector;
    private Vector2 prevPosition;

    public Rigidbody2D Rigid2D { get; private set; }
    private SpriteRenderer spriter;
    private Animator animator;

    private void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = inputVector * moveSpeed * Time.fixedDeltaTime;
        Rigid2D.MovePosition( Rigid2D.position + delta );

        Direction = ( Rigid2D.position - prevPosition ).normalized;
        prevPosition = Rigid2D.position;
    }

    private void LateUpdate()
    {
        animator.SetFloat( "MoveSpeed", inputVector.sqrMagnitude );

        if ( inputVector.x != 0f )
        {
            spriter.flipX = inputVector.x < 0f;
        }
    }

    private void OnMove( InputValue _value )
    {
        inputVector = _value.Get<Vector2>();

        OnMoveEvent?.Invoke( _value );
    }

    private void OnAttack( InputValue _value )
    {
        OnAttackEvent?.Invoke( _value );
    }
}

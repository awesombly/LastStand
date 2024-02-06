using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public Vector2 Direction { get; private set; }
    public bool IsAttackHolded { get; private set; }

    [SerializeField]
    private float moveSpeed;

    private Vector2 inputVector;
    private Vector2 prevPosition;

    public Rigidbody2D Rigid2D { get; private set; }
    private SpriteRenderer spriter;
    private Animator animator;

    public Action<InputValue> OnMoveEvent;
    public Action OnAttackPressEvent;
    public Action OnAttackReleaseEvent;
    public Action OnReloadEvent;

    #region Unity Callback
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
    #endregion

    #region InputSystem Callback
    private void OnMove( InputValue _value )
    {
        inputVector = _value.Get<Vector2>();

        OnMoveEvent?.Invoke( _value );
    }

    private void OnAttackPress()
    {
        IsAttackHolded = true;
        OnAttackPressEvent?.Invoke();
    }

    private void OnAttackRelease()
    {
        IsAttackHolded = false;
        OnAttackReleaseEvent?.Invoke();
    }

    private void OnReload()
    {
        OnReloadEvent?.Invoke();
    }
    #endregion
}

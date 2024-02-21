using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionReceiver : MonoBehaviour
{
    public bool IsAttackHolded { get; private set; }
    public Vector2 InputVector { get; private set; }

    public Action<InputValue> OnMoveEvent;
    public Action OnAttackPressEvent;
    public Action OnAttackReleaseEvent;
    public Action OnReloadEvent;
    public Action OnDodgeEvent;

    #region InputSystem Callback
    private void OnMove( InputValue _value )
    {
        InputVector = _value.Get<Vector2>();

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

    private void OnDodge()
    {
        OnDodgeEvent?.Invoke();
    }
    #endregion
}

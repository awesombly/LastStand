using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionReceiver : MonoBehaviour
{
    public bool IsAttackHolded { get; private set; }
    public Vector2 InputVector { get; private set; }

    public event Action<Vector2> OnMoveEvent;
    public event Action OnAttackPressEvent;
    public event Action OnAttackReleaseEvent;
    public event Action OnReloadEvent;
    public event Action OnDodgeEvent;
    public event Action<int/*index*/> OnSwapWeaponEvent;
    public event Action OnInteractionEvent;

    #region InputSystem Callback
    private void OnMove( InputValue _value )
    {
        InputVector = _value.Get<Vector2>();

        OnMoveEvent?.Invoke( InputVector );
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

    private void OnSwapWeapon( InputValue _value )
    {
        int value =  Mathf.RoundToInt( _value.Get<float>() );
        OnSwapWeaponEvent?.Invoke( value );
    }

    private void OnInteraction()
    {
        OnInteractionEvent?.Invoke();
    }
    #endregion
}

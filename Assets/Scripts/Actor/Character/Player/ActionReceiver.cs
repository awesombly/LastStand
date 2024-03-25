using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class ActionReceiver : MonoBehaviour
{
    public bool IsAttackHolded { get; private set; }
    public Vector2 InputVector { get; private set; }

    #region Events
    public event Action<Vector2> OnMoveEvent;
    public event Action<Vector2> OnAimEvent;        // VirtualPad용
    public event Action OnAttackPressEvent;
    public event Action OnAttackReleaseEvent;
    public event Action OnReloadEvent;
    public event Action OnDodgeEvent;
    public event Action<int/*index*/> OnSwapWeaponEvent;
    public event Action OnNextWeaponEvent;
    public event Action OnPrevWeaponEvent;
    public event Action OnInteractionEvent;
    #endregion

    #region Camera
    private float targetOrthoSize;
    private float prevOrthoSize;
    private Coroutine orthoCoroutine = null;

    private PixelPerfectCamera ppCamera;
    private CinemachineVirtualCamera virtualCamera;
    private Cinemachine.CinemachinePixelPerfect pixelPerfect;
    private CinemachineConfiner2D confiner2D;
    #endregion

    #region Unity Callback
    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
#endif
    }

    private void Start()
    {
        Player player = GetComponent<Player>();
        if ( !player.IsLocal )
        {
            return;
        }

        ppCamera = Camera.main.GetComponent<PixelPerfectCamera>();
        virtualCamera = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        pixelPerfect = virtualCamera.GetComponent<Cinemachine.CinemachinePixelPerfect>();
        confiner2D = virtualCamera.GetComponent<Cinemachine.CinemachineConfiner2D>();
        if ( ppCamera is null || virtualCamera is null || pixelPerfect is null || confiner2D is null )
        {
            Debug.LogError( $"Not Exist Components. {ppCamera}, {virtualCamera}, {pixelPerfect}, {confiner2D}" );
        }

        confiner2D.InvalidateCache();
        targetOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        prevOrthoSize = Camera.main.orthographicSize;
    }

    private void FixedUpdate()
    {
        // CinemachinePixelPerpect 사용시 카메라 OrthoSize가 일정 구간이 아니면 바뀌지 않는다.(픽셀 보정용인듯)
        // 그대로 OrthoSize 변경시 끊기는 느낌이 들어, 바뀌는 구간마다 보간해준다
        float correctSize = ppCamera.CorrectCinemachineOrthoSize( targetOrthoSize );
        if ( !Mathf.Approximately( prevOrthoSize, correctSize ) )
        {
            if ( orthoCoroutine is not null )
            {
                StopCoroutine( orthoCoroutine );
            }
            prevOrthoSize = correctSize;
            orthoCoroutine = StartCoroutine( LerpCameraOrthoSize( correctSize ) );
        }
    }

    private void OnDisable()
    {
        IsAttackHolded = false;
        InputVector = Vector2.zero;
    }
    #endregion

    public void AddVirtualStickEvent( OnScreenStickCustom _moveStick, OnScreenStickCustom _aimStick )
    {
        _moveStick.onJoystickMove.AddListener( OnMoveImplement );
        _moveStick.onJoystickRelease.AddListener( OnMoveRelease );
        _aimStick.onJoystickMove.AddListener( OnAimImplement );
        _aimStick.onJoystickRelease.AddListener( OnAimRelease );
    }

    public void RemoveVirtualStickEvent( OnScreenStickCustom _moveStick, OnScreenStickCustom _aimStick )
    {
        _moveStick.onJoystickMove.RemoveListener( OnMoveImplement );
        _moveStick.onJoystickRelease.RemoveListener( OnMoveRelease );
        _aimStick.onJoystickMove.RemoveListener( OnAimImplement );
        _aimStick.onJoystickRelease.RemoveListener( OnAimRelease );
    }

    private IEnumerator LerpCameraOrthoSize( float _targetSize )
    {
        pixelPerfect.enabled = false;
        float orthoSize = virtualCamera.m_Lens.OrthographicSize;

        while ( !Mathf.Approximately( orthoSize, _targetSize ) )
        {
            orthoSize = Mathf.MoveTowards( orthoSize, _targetSize, Time.deltaTime * 3f );
            virtualCamera.m_Lens.OrthographicSize = orthoSize;
            // OrthoSize 변경시 충돌경계 업데이트가 안되어 추가
            confiner2D.InvalidateCache();
            yield return YieldCache.WaitForEndOfFrame;
        }

        pixelPerfect.enabled = true;
    }

    #region InputSystem Callback
    private void OnMoveImplement( Vector2 _value )
    {
        InputVector = _value;
        OnMoveEvent?.Invoke( InputVector );
    }

    private void OnMove( InputValue _value )
    {
        OnMoveImplement( _value.Get<Vector2>() );
    }

    private void OnMoveRelease()
    {
        OnMoveImplement( Vector2.zero );
    }

    private void OnAimImplement( Vector2 _direction )
    {
        OnAimEvent?.Invoke( _direction );

        // Attack Input
        if ( _direction.sqrMagnitude > float.Epsilon )
        {
            IsAttackHolded = true;
            OnAttackPressEvent?.Invoke();
        }
        else
        {
            IsAttackHolded = false;
            OnAttackReleaseEvent?.Invoke();
        }
    }

    private void OnAim( InputValue _value )
    {
        OnAimImplement( _value.Get<Vector2>() );
    }

    private void OnAimRelease()
    {
        OnAimImplement( Vector2.zero );
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

    private void OnSwapWeaponPad( InputValue _value )
    {
        int value = Mathf.RoundToInt( _value.Get<float>() );
        OnSwapWeaponEvent?.Invoke( value );
    }

    private void OnPrevWeapon()
    {
        OnPrevWeaponEvent?.Invoke();
    }

    private void OnNextWeapon()
    {
        OnNextWeaponEvent?.Invoke();
    }

    private void OnInteraction()
    {
        OnInteractionEvent?.Invoke();
    }

    private void OnCameraZoom()
    {
        Vector3 mousePos = Camera.main.ScreenToViewportPoint( Input.mousePosition );
        if ( mousePos.x >= 0 && mousePos.x <= 1f && mousePos.y >= 0f && mousePos.y <= 1f )
        {
            targetOrthoSize = Mathf.Clamp( targetOrthoSize - Mouse.current.scroll.value.y * 0.005f, 1f, 15f );
        }
    }
    #endregion
}

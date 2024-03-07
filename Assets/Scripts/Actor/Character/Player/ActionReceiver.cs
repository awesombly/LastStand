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

    public event Action<Vector2> OnMoveEvent;
    public event Action OnAttackPressEvent;
    public event Action OnAttackReleaseEvent;
    public event Action OnReloadEvent;
    public event Action OnDodgeEvent;
    public event Action<int/*index*/> OnSwapWeaponEvent;
    public event Action OnInteractionEvent;

    #region Camera
    private float targetOrthoSize;
    private float prevOrthoSize;
    private Coroutine orthoCoroutine = null;

    private PixelPerfectCamera ppCamera;
    private CinemachineVirtualCamera virtualCamera;
    private Cinemachine.CinemachinePixelPerfect pixelPerfect;
    private CinemachineConfiner2D confiner2D;
    #endregion

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

    private void OnDisable()
    {
        IsAttackHolded = false;
        InputVector = Vector2.zero;
    }

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

    private void OnCameraZoom( InputValue _value )
    {
        targetOrthoSize = Mathf.Clamp( targetOrthoSize + _value.Get<float>() * 0.01f, 1f, 15f );
    }
    #endregion
}

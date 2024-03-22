using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class StickHoldEffecter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] float duration;
    [Range( 1f, 2f )]
    [SerializeField] float scaleMultiplier;
    [SerializeField] Image targetImage;
    [SerializeField] Color targetColor;
    // UI 이벤트가 한쪽에서만 발생해서 전달하는 용도
    [SerializeField] OnScreenStick screenStick;

    private Vector3 targetSize;
    private Vector3 originScale;
    private Color originColor;
    private Sequence downSequence;
    private Sequence upSequence;

    private void Awake()
    {
        targetSize = new Vector3( scaleMultiplier, scaleMultiplier, 1f );

        originScale = transform.localScale;
        originColor = targetImage.color;
    }

    private void Start()
    {
        downSequence = DOTween.Sequence().Pause().SetAutoKill( false );
        downSequence.Append( transform.DOScale( targetSize, duration ) )
            .Join( targetImage.DOColor( targetColor, duration ) );

        upSequence = DOTween.Sequence().Pause().SetAutoKill( false );
        upSequence.Append( transform.DOScale( originScale, duration ) )
            .Join( targetImage.DOColor( originColor, duration ) );
    }

    private void OnDestroy()
    {
        DOTween.Kill( downSequence );
        DOTween.Kill( upSequence );
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        screenStick.OnPointerDown( eventData );
        downSequence.Restart();
    }

    public void OnPointerUp( PointerEventData eventData )
    {
        screenStick.OnPointerUp( eventData );
        upSequence.Restart();
    }

    public void OnDrag( PointerEventData eventData )
    {
        screenStick.OnDrag( eventData );
    }
}

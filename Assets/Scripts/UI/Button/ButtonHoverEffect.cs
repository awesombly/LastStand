using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent( typeof( Image ) )]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    public float targetAlpha = .015f;
    public float duration    = .25f;

    private void Awake()
    {
        if ( TryGetComponent( out image ) )
             image.color = new Color( 1f, 1f, 1f, 0f );
    }

    private void OnDestroy()
    {
        DOTween.Kill( image );
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        image.DOFade( targetAlpha, duration );
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        image.DOFade( 0f, duration );
    }
}

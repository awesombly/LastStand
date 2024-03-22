using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickEffecter : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] float duration;
    [Range( 1f, 2f )]
    [SerializeField] float scaleMultiplier;
    [SerializeField] Image targetImage;
    [SerializeField] Color targetColor;

    private Vector3 targetSize;
    private Vector3 originScale;
    private Color originColor;
    private Sequence sequence;

    private void Awake()
    {
        targetSize = new Vector3( scaleMultiplier, scaleMultiplier, 1f );

        originScale = transform.localScale;
        originColor = targetImage.color;
    }

    private void Start()
    {
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( transform.DOScale( targetSize, duration ) )
            .Join( targetImage.DOColor( targetColor, duration ) )
            .OnComplete( () =>
            {
                Init();
            } );
    }

    private void OnDestroy()
    {
        DOTween.Kill( sequence );
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        Init();
        sequence.Restart();
    }

    private void Init()
    {
        transform.localScale = originScale;
        targetImage.color = originColor;
    }
}

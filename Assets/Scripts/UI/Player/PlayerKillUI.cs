using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerKillUI : MonoBehaviour
{
    public GameObject canvas;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI deadPlayerName;

    private Sequence effectSeq;

    private void Start()
    {
        GameManager.OnChangeLocalPlayer += ( Player _old, Player _new ) =>
        {
            if ( _old == _new )
                 return;

            if ( !ReferenceEquals( _old, null ) )
                 _old.OnPlayerKill -= OnKill;

            if ( !ReferenceEquals( _new, null ) )
                 _new.OnPlayerKill += OnKill;
        };

        effectSeq = DOTween.Sequence().SetAutoKill( false ).Pause();
        effectSeq.AppendCallback( () => canvasGroup.alpha = 0f ).
                  Append( canvasGroup.DOFade( 1f, .5f ) ).
                  AppendInterval( 2.5f ).
                  Append( canvasGroup.DOFade( 0f, .5f ) );
    }

    private void OnKill( Player _player )
    {
        canvas.SetActive( true );
        deadPlayerName.text = _player.Nickname;
        effectSeq.Restart();
    }
}

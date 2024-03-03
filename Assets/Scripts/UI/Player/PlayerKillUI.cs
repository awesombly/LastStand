using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerKillUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI deadPlayerName;

    private Sequence effectSeq;

    private void Start()
    {
        canvasGroup.alpha = 0f;

        effectSeq = DOTween.Sequence().SetAutoKill( false ).Pause();
        effectSeq.AppendCallback( () => canvasGroup.alpha = 0f ).
                  Append( canvasGroup.DOFade( 1f, .5f ) ).
                  AppendInterval( 2.5f ).
                  Append( canvasGroup.DOFade( 0f, .5f ) );

        GameManager.OnChangeLocalPlayer += ( Player _old, Player _new ) =>
        {
            if ( _old == _new )
                 return;

            if ( !ReferenceEquals( _old, null ) )
                 _old.OnPlayerKill -= OnKill;

            if ( !ReferenceEquals( _new, null ) )
                 _new.OnPlayerKill += OnKill;
        };
    }

    private void OnKill( Player _player )
    {
        deadPlayerName.text = _player.Nickname;
        effectSeq.Restart();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PlayerKillUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI deadPlayerName;

    private Sequence effectSeq;
    private Player target;

    private void Start()
    {
        canvasGroup.alpha = 0f;

        effectSeq = DOTween.Sequence().SetAutoKill( false ).Pause();
        effectSeq.AppendCallback( () => canvasGroup.alpha = 0f ).
                  Append( canvasGroup.DOFade( 1f, .5f ) ).
                  AppendInterval( 2.5f ).
                  Append( canvasGroup.DOFade( 0f, .5f ) );

        GameManager.OnChangeLocalPlayer += AddEvents;
    }

    private void OnDestroy()
    {
        GameManager.OnChangeLocalPlayer -= AddEvents;
        DOTween.Kill( effectSeq );
        DOTween.Kill( canvasGroup );
        if ( target is not null )
             target.OnPlayerKill -= OnKill;
    }

    private void AddEvents( Player _old, Player _new )
    {
        if ( _old == _new )
             return;

        if ( _old is not null )
            _old.OnPlayerKill -= OnKill;

        if ( _new is not null )
        {
            target = _new;
            _new.OnPlayerKill += OnKill;
        }
    }

    private void OnKill( Player _player )
    {
        deadPlayerName.text = _player.Nickname;
        effectSeq?.Restart();
    }
}

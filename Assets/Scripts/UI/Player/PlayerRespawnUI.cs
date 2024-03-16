using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PlayerRespawnUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI respawnTime;
    public TextMeshProUGUI attackerName;
    public Slider respawnBar;
    public GameObject fill;
    public Image background;

    private readonly Color StartColor = new Color( 1f, 0f, 0f, .9f );
    private readonly Color EndColor   = new Color( 0f, 0f, 0f, .9f );

    private CanvasGroup group;
    private Player target;

    private void Awake()
    {
        GameManager.OnBeginRespawn      += BeginRespawn;
        GameManager.OnEndRespawn        += EndRespawn;
        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;

        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        panel.SetActive( false );
    }

    private void OnDestroy()
    {
        GameManager.OnBeginRespawn      -= BeginRespawn;
        GameManager.OnEndRespawn        -= EndRespawn;
        GameManager.OnChangeLocalPlayer -= OnChangeLocalPlayer;

        DOTween.Kill( group );
        DOTween.Kill( background );

        if ( target is not null )
        {
            target.PlayerUI.respawnDelay.OnChangeCurrent -= OnChangeRespawnDelay;
            target.OnPlayerDead -= OnPlayerDead;
        }
    }

    private void BeginRespawn()
    {
        panel.SetActive( true );
        fill.SetActive( true );
        background.color = StartColor;

        group.alpha = 0f;
        group.DOFade( 1f, .125f ).OnComplete( () => background.DOColor( EndColor, .5f ) );
    }

    private void EndRespawn()
    {
        fill.SetActive( false );
        group.DOFade( 0f, .25f ).OnComplete( () => panel.SetActive( false ) );
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _old == _new )
             return;

        if ( _old is not null )
        {
            _old.PlayerUI.respawnDelay.OnChangeCurrent -= OnChangeRespawnDelay;
            _old.OnPlayerDead -= OnPlayerDead;
        }

        if ( _new is not null )
        {
            target = _new;
            _new.PlayerUI.respawnDelay.OnChangeCurrent += OnChangeRespawnDelay;
            _new.OnPlayerDead += OnPlayerDead;
        }
    }

    private void OnChangeRespawnDelay( float _old, float _new, float _max )
    {
        respawnTime.text = System.MathF.Round( _new, 2 ).ToString( "F2" );
        respawnBar.value = _new / _max;
    }

    private void OnPlayerDead( Player _attacker )
    {
        if ( _attacker is not null )
        {
            attackerName.text = _attacker.Nickname;
        }
    }
}

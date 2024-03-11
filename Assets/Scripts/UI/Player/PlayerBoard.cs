using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PlayerBoard : MonoBehaviour
{
    public Player Target { get; private set; }

    [Header( "< Infomagtion >" )]
    public GameObject      crown;
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    [Header( "< Health >")]
    public Slider health;
    public Slider healthLerp;
    public GameObject healthFill, healthLerpFill;

    [Header( "< Effect >" )]
    public  Image foreground;
    private Image background;
    private readonly Color StartColor = new Color( 1f, 0f, 0f, .9f );
    private readonly Color EndColor   = new Color( 0f, 0f, 0f, .9f );

    private RectTransform rt;
    private bool isFirstSpawn = true;

    private void Awake()
    {
        rt         = transform as RectTransform;
        background = GetComponent<Image>();
    }

    private void OnDestroy()
    {
        if ( Target is null )
             return;

        Target.OnPlayerDead    -= PlayerDead;
        Target.OnPlayerRespawn -= PlayerRespawn;
    }

    public void ActiveCrown( bool _active ) => crown.SetActive( _active );

    public void AddEvents( Player _player )
    {
        Target = _player;
        Target.Board = this;

        Target.OnPlayerDead    += PlayerDead;
        Target.OnPlayerRespawn += PlayerRespawn;

        ActiveCrown( _player.IsHost );
        nickname.text   = Target.Nickname;
        killCount.text  = $"{Target.KillScore}";
        deathCount.text = $"{Target.DeathScore}";

        if ( Target.IsDead )
        {
            foreground.gameObject.SetActive( true );
            foreground.color = EndColor;
            healthFill.SetActive( false );
            healthLerpFill.SetActive( false );
        }
        else
        {
            foreground.color = Color.clear;
            foreground.gameObject.SetActive( false );
            healthFill.SetActive( true );
            healthLerpFill.SetActive( true );
        }

        if ( Target.IsLocal ) background.color = new Color( .5f, 1f, .5f );
        else                  background.color = new Color( 1f, .5f, .5f );

        UpdateHealth( Target.Health );
        UpdateHealthLerp( Target.HealthLerp );
    }

    public void RemoveEvents()
    {
        if ( Target is null )
             return;

        Target.OnPlayerDead    -= PlayerDead;
        Target.OnPlayerRespawn -= PlayerRespawn;
        Target = null;
    }

    public void PlayerDead( Player _attacker )
    {
        foreground.gameObject.SetActive( true );
        foreground.color = StartColor;
        foreground.DOColor( EndColor, .5f );
    }

    public void PlayerRespawn()
    {
        if ( isFirstSpawn )
        {
            isFirstSpawn = false;
            return;
        }

        healthFill.SetActive( true );
        healthLerpFill.SetActive( true );

        foreground.DOColor( Color.clear, .5f )
                  .OnComplete( () => foreground.gameObject.SetActive( false ) );

    }

    public void UpdateHealth( float _health )
    {
        health.value = _health;
        if ( _health <= float.Epsilon )
        {
            healthFill.SetActive( false );
            healthLerpFill.SetActive( false );
        }
    }

    public void MoveToPosition( Vector2 _pos ) => rt.DOAnchorPos( _pos, .5f );

    public void UpdateHealthLerp( float _healthLerp ) => healthLerp.value = _healthLerp;

    public void UpdateKillCount( int _killCount ) => killCount.text = $"{_killCount}";
    
    public void UpdateDeathCount( int _deathCount ) => deathCount.text = $"{_deathCount}";
}
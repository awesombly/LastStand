using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PlayerBoard : MonoBehaviour
{
    [Header( "< Infomagtion >" )]
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    public Slider health, healthLerp;
    public GameObject healthFill, healthLerpFill;

    [Header( "< Effect >" )]
    public Image deadBackground;
    public static readonly Color PlayerDeadBGStartColor = new Color( 1f, 0f, 0f, .9f );
    public static readonly Color PlayerDeadBGEndColor   = new Color( 0f, 0f, 0f, .9f );

    private Image image;
    private RectTransform rt;

    private void Awake()
    {
        rt    = transform as RectTransform;
        image = GetComponent<Image>();
    }

    public void UpdateInfomation( in Player _player )
    {
        nickname.text   = _player.Nickname;
        killCount.text  = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";

        _player.OnPlayerDead    += PlayerDead;
        _player.OnPlayerRespawn += PlayerRespawn;

        if ( _player.IsLocal ) image.color = new Color( .5f, 1f, .5f );
        else                   image.color = new Color( 1f, .5f, .5f );

        _player.InitBoardUI( this );
    }

    public void PlayerDead( Player _attacker )
    {
        deadBackground.gameObject.SetActive( true );
        deadBackground.color = PlayerDeadBGStartColor;
        deadBackground.DOColor( PlayerDeadBGEndColor, .5f );
    }

    public void PlayerRespawn()
    {
        deadBackground.DOColor( Color.clear, .5f )
                      .OnComplete( () => deadBackground.gameObject.SetActive( false ) );

        healthFill.SetActive( true );
        healthLerpFill.SetActive( true );
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
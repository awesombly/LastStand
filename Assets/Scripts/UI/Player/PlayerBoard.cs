using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoard : MonoBehaviour
{
    [Header( "< Infomagtion >" )]
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    public Slider health, healthLerp;

    [Header( "< Effect >" )]
    public Image deadBackground;
    public static readonly Color PlayerDeadBGStartColor = new Color( 1f, 0f, 0f, .9f );
    public static readonly Color PlayerDeadBGEndColor   = new Color( 0f, 0f, 0f, .9f );
    public void Initialize( in Player _player )
    {
        nickname.text   = _player.Nickname;
        killCount.text  = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";

        _player.OnPlayerDead    += PlayerDead;
        _player.OnPlayerRespawn += PlayerRespawn;

        _player.InitBoardUI( this );
    }

    public void PlayerDead()
    {
        deadBackground.gameObject.SetActive( true );
        deadBackground.color = PlayerDeadBGStartColor;
        deadBackground.DOColor( PlayerDeadBGEndColor, .5f );
    }

    public void PlayerRespawn()
    {
        deadBackground.DOColor( Color.clear, .5f )
                      .OnComplete( () => deadBackground.gameObject.SetActive( false ) );
    }

    public void UpdateHealth( float _health ) => health.value = _health;

    public void UpdateHealthLerp( float _healthLerp ) => healthLerp.value = _healthLerp;

    public void UpdateKillCount( int _killCount ) => killCount.text = $"{_killCount}";
    
    public void UpdateDeathCount( int _deathCount ) => deathCount.text = $"{_deathCount}";
}
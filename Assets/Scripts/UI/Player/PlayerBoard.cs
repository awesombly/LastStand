using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoard : MonoBehaviour
{
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    public Slider health, healthLerp;

    public void Initialize( in Player _player )
    {
        nickname.text   = _player.Nickname;
        killCount.text  = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";
        _player.InitBoardUI( this );
    }

    public void UpdateHealth( float _health ) => health.value = _health;

    public void UpdateHealthLerp( float _healthLerp ) => healthLerp.value = _healthLerp;

    public void UpdateKillCount( int _killCount ) => killCount.text = $"{_killCount}";
    
    public void UpdateDeathCount( int _deathCount ) => deathCount.text = $"{_deathCount}";
}
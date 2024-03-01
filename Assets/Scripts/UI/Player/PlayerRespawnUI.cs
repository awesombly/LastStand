using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRespawnUI : MonoBehaviour
{
    public TextMeshProUGUI respawnTime;
    public Slider respawnBar;
    public Image backGround;
    private Color bgColor;

    private void Awake()
    {
        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;

        bgColor = new Color( 0f, 0f, 0f, 0.9f );
        gameObject.SetActive( false );
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( !ReferenceEquals( _old, null ) )
        {
            _old.PlayerUI.respawnDelay.OnChangeCurrent -= OnChangeRespawnDelay;
        }

        if ( !ReferenceEquals( _new, null ) )
        {
            _new.PlayerUI.respawnDelay.OnChangeCurrent += OnChangeRespawnDelay;
        }
    }

    private void OnChangeRespawnDelay( float _old, float _new, float _max )
    {
        gameObject.SetActive( _new > 0f );
        respawnTime.text = System.MathF.Round( _new, 2 ).ToString();
        respawnBar.value = _new / _max;
        backGround.color = bgColor;
    }
}
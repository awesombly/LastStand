using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultBoard : MonoBehaviour
{
    public GameObject trophy;
    public Animator animatior;
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;
    public SpriteRenderer  rdr;

    private void Awake()
    {
        rdr.color = new Color( 1f, 1f, 1f, 0f );
    }

    public void Initialize( Player _player, bool _isWinner, float _fadeTime )
    {
        PlayerSO playerData = GameManager.Inst.GetPlayerSO( _player.PlayerType );
        animatior.runtimeAnimatorController = playerData.playerAC;
        animatior.SetBool( AnimatorParameters.IsActionBlocked, true );

        if ( _isWinner )
        {
            animatior.SetTrigger( AnimatorParameters.DanceAction );
            trophy.SetActive( true );
        }
        else
        {
            animatior.SetTrigger( AnimatorParameters.DefeatAction );
            trophy.SetActive( false );
        }

        nickname.text   = _player.Nickname;
        killCount.text  = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";

        rdr.DOFade( 1f, _fadeTime );
    }
}

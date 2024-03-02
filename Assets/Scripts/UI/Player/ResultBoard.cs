using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultBoard : MonoBehaviour
{
    public Animator animatior;
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    public void Initialize( Player _player, bool _isWinner )
    {
        animatior.SetBool( PlayerAnimator.AnimatorParameters.IsActionBlocked, true );
        if ( _isWinner )
        {
            animatior.SetTrigger( PlayerAnimator.AnimatorParameters.DanceAction );
            nickname.text = $"~ {_player.Nickname} ~";

        }
        else
        {
            animatior.SetTrigger( PlayerAnimator.AnimatorParameters.DefeatAction );
            nickname.text = _player.Nickname;
        }
        killCount.text = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";
    }
}

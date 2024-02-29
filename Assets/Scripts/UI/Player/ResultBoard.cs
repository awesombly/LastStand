using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultBoard : MonoBehaviour
{
    public Image playerImage;
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI deathCount;

    public void Initialize( Player _player, bool _isWinner )
    {
        playerImage.sprite = _player.GetComponent<SpriteRenderer>().sprite;
        nickname.text = _isWinner ? $"~ {_player.Nickname} ~" : _player.Nickname;
        killCount.text = $"{_player.KillScore}";
        deathCount.text = $"{_player.DeathScore}";
    }
}

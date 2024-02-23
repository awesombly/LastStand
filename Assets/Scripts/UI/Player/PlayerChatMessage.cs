using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChatMessage : MonoBehaviour
{
    [SerializeField]
    private Image background;
    [SerializeField]
    private TextMeshProUGUI message;

    private readonly float MSGEnableTime = 3f;
    private bool isPlaying;
    private float time;
    private float alpha;

    public void UpdateMessage( string _message )
    {
        isPlaying = true;
        time = alpha = 1f;
        message.alpha = 1f;
        background.color = Color.white;
        message.text = _message;
    }

    private void Update()
    {
        if ( !isPlaying )
             return;

        time += Time.deltaTime;
        if ( time > MSGEnableTime )
        {
            alpha -= Time.deltaTime;
            background.color = new Color( 1f, 1f, 1f, alpha );
            message.alpha = alpha;
            if ( alpha <= 0 )
            {
                isPlaying = false;
                gameObject.SetActive( false );
            }
        }
    }
}
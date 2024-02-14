using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatMsg : MonoBehaviour, WNS.IObjectPool<ChatMsg>
{
    public WNS.ObjectPool<ChatMsg> pool { get; set; }
    [SerializeField]
    private TextMeshProUGUI text;
    private readonly float MSGEnableTime = 3f;
    private bool isPlaying;
    private float time;

    public void Initialize( CHAT_MESSAGE _message )
    {
        //transform.SetAsFirstSibling();
        transform.localScale = Vector3.one;
        text.text = $"<{_message.nickname}>  {_message.message}";
        text.alpha = 1f;
        isPlaying = true;
        time = 0f;
    }

    private void Update()
    {
        if ( !isPlaying )
             return;

        time += Time.deltaTime;
        if ( time > MSGEnableTime )
        {
            text.alpha -= Time.deltaTime;
            if ( text.alpha <= 0 )
            {
                pool.Despawn( this );
                isPlaying = false;
            }
        }
    }
}

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

    public void Initialize( string _message )
    {
        //transform.SetAsFirstSibling();
        transform.localScale = Vector3.one;
        text.alpha = 1f;
        text.text = _message;
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

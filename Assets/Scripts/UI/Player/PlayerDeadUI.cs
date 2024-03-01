using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent( typeof( CanvasGroup ) )]
public class PlayerDeadUI : MonoBehaviour, WNS.IObjectPool<PlayerDeadUI>
{
    public WNS.ObjectPool<PlayerDeadUI> pool { get; set; } 
    public float waitTime = 3f;
    public float fadeSpeed = 10f;
    public TextMeshProUGUI nickname;
    private CanvasGroup group;
    private bool isPlaying, isReverse;
    private float time;

    private void Awake()
    {
        if ( TryGetComponent( out group ) )
        {
            group.blocksRaycasts = false;
        }
    }

    public void Initialize( in Player _player )
    {
        transform.SetAsLastSibling();
        nickname.text = _player.Nickname;
        group.alpha = 0f;
        isPlaying = true;
        isReverse = false;
        time = 0f;
    }


    private void Update()
    {
        if ( !isPlaying )
             return;

        if ( !isReverse )
        {
            group.alpha += Time.deltaTime * fadeSpeed;
            if ( group.alpha >= 1f )
            {
                isReverse = true;
                group.alpha = 1f;
            }
        }
        else
        {
            time += Time.deltaTime;
            if ( time > waitTime )
            {
                group.alpha -= Time.deltaTime * fadeSpeed;
                if ( group.alpha <= 0f )
                {
                    pool.Despawn( this );
                    isPlaying = false;
                }
            }
        }
    }
}

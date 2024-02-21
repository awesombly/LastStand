using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WNS;

[RequireComponent( typeof( AudioSource ) )]
public class SoundChannel : MonoBehaviour, WNS.IObjectPool<SoundChannel>
{ 
    public ObjectPool<SoundChannel> pool { get; set; }
    [SerializeField]
    private AudioSource channel;
    private bool isPlaying;

    private void Awake()
    {
        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "AudioSource is not found" );
    }

    public void Play( AudioClip _clip )
    {
        channel.clip = _clip;
        channel.Play();
        isPlaying = true;
    }

    private void Update()
    {
        if ( !isPlaying )
             return;

        if ( !channel.isPlaying )
        {
            channel.clip = null;
            isPlaying    = false;
            pool.Despawn( this );
        }
    }
}
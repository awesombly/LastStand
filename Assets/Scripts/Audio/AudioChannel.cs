using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( AudioSource ) )]
public class AudioChannel : MonoBehaviour, WNS.IObjectPool<AudioChannel>
{
    #region Variables
    public WNS.ObjectPool<AudioChannel> pool { get; set; }
    public AudioSource channel;
    private bool isPlaying;

    #region Properties
    public AudioClip Clip
    {
        get => channel.clip;
        set => channel.clip = value;
    }

    public float Volume
    {
        get => channel.volume;
        set
        {
            if ( value < 0f || value > 1f )
            {
                Debug.LogWarning( "The volume must have a value between 0 and 1" );
                return;
            }
            channel.volume = value;
        }
    }
    #endregion
    #endregion

    #region Unity Callback
    private void Awake()
    {
        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "No AudioSource Component were found for the AudioChannel" );
    }

    private void Update()
    {
        if ( !isPlaying )
             return;

        if ( !channel.isPlaying )
             pool.Despawn( this );
    }

    private void OnEnable()
    {
        channel.Stop();
        channel.outputAudioMixerGroup = null;
        channel.volume = 1f;
        channel.clip = null;
        isPlaying    = false;
    }
    #endregion


    public void Play()
    {
        if ( isPlaying )
        {
            Debug.LogWarning( $"Audio is already playing" );
            return;
        }

        if ( channel.clip == null )
        {
            Debug.LogWarning( $"The SoundClip is not registered to the AudioSource" );
            return;
        }

        channel.Play();
        isPlaying = true;
    }
}
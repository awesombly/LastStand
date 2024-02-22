using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent( typeof( AudioSource ) )]
public class AudioChannel : MonoBehaviour, WNS.IObjectPool<AudioChannel>
{
    #region Variables
    public WNS.ObjectPool<AudioChannel> pool { get; set; }
    public AudioSource audioSource;
    private bool isPlaying;

    #region Properties
    public AudioClip Clip
    {
        get => audioSource.clip;
        set => audioSource.clip = value;
    }

    public float Volume
    {
        get => audioSource.volume;
        set => audioSource.volume = value < 0f ? 0f :
                                    value > 1f ? 1f : value;
    }

    public AudioMixerGroup MixerGroup
    {
        get => audioSource.outputAudioMixerGroup;
        set => audioSource.outputAudioMixerGroup = value;
    }
    #endregion
    #endregion

    #region Unity Callback
    private void Awake()
    {
        if ( !TryGetComponent( out audioSource ) )
             Debug.LogError( "No AudioSource Component were found for the AudioChannel" );
    }

    private void Update()
    {
        if ( !isPlaying )
             return;

        if ( !audioSource.isPlaying )
             pool.Despawn( this );
    }

    private void OnEnable()
    {
        audioSource.Stop();
        audioSource.outputAudioMixerGroup = null;
        audioSource.volume = 1f;
        audioSource.clip = null;

        transform.position = Vector3.zero;
        isPlaying = false;
    }
    #endregion

    public void Play()
    {
        if ( isPlaying )
        {
            Debug.LogWarning( $"Audio is already playing" );
            return;
        }

        if ( audioSource.clip == null )
        {
            Debug.LogWarning( $"The SoundClip is not registered to the AudioSource" );
            return;
        }

        audioSource.Play();
        isPlaying = true;
    }
}
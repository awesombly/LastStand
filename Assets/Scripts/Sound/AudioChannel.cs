using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( AudioSource ) )]
public class AudioChannel : MonoBehaviour, WNS.IObjectPool<AudioChannel>
{ 
    public WNS.ObjectPool<AudioChannel> pool { get; set; }
    public AudioSource channel;

    private void Awake()
    {
        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "AudioSource is not found" );
    }

    private void OnEnable()
    {
        channel.Stop();
        channel.outputAudioMixerGroup = null;
        channel.volume = 1f;
        channel.clip = null;
    }

    private IEnumerator FadeVolume( float _start, float _end, float _t )
    {
        channel.Pause();

        if ( Global.Mathematics.Abs( _start - _end ) < float.Epsilon )
        {
            channel.volume = _end;
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        channel.volume =_start;
        channel.UnPause();
        while ( _start < _end ? elapsedVolume < _end : // FADEIN
                                elapsedVolume > _end ) // FADEOUT
        {
            yield return YieldCache.WaitForEndOfFrame;
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            channel.volume = elapsedVolume;
        }
        channel.volume = _end;
    }

    private void Update()
    {
        if ( !channel.isPlaying )
        {
     
            pool.Despawn( this );
        }
    }
}
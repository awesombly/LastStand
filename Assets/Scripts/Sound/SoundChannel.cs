using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent( typeof( AudioSource ) )]
public class SoundChannel : MonoBehaviour, WNS.IObjectPool<SoundChannel>
{ 
    public WNS.ObjectPool<SoundChannel> pool { get; set; }
    [SerializeField]
    private AudioSource channel;
    private Coroutine fadeCor;

    private float length;

    //public bool IsPlaying { get; private set; }
    public bool Loop
    {
        get => channel.loop;
        set => channel.loop = value;
    }
    public float Volume
    {
        get => channel.volume;
        set => channel.volume = value;
    }

    private void Awake()
    {
        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "AudioSource is not found" );
    }

    public void Play( AudioClip _clip )
    {
        channel.clip = _clip;
        length       = channel.clip.length;
        channel.Play();
    }

    public void Stop()
    {
        if ( channel.isPlaying )
        {
            Clear();
            pool.Despawn( this );
        }
    }

    public void Fade( float _start, float _end, float _t )
    {
        StopEffect();
        fadeCor = StartCoroutine( FadeVolume( _start, _end, _t ) );
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
        if ( Global.Mathematics.Abs( channel.time - length ) <= float.Epsilon )
        {
            Clear();
            pool.Despawn( this );
        }
    }

    private void Clear()
    {
        StopEffect();
        channel.volume = 1f;
        length         = 0f;
        channel.clip   = null;
    }

    private void StopEffect()
    {
        if ( !ReferenceEquals( fadeCor, null ) )
        {
            StopCoroutine( fadeCor );
            fadeCor = null;
        }
    }
}
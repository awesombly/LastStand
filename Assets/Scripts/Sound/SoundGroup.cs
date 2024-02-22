using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGroup<T, U> where T : System.Enum where U : System.Enum
{
    public class SoundInfo
    {
        private Dictionary<U, AudioClip> datas = new Dictionary<U, AudioClip>();

        public AudioClip this[U type] => datas.ContainsKey( type ) ? datas[type] : null;

        public void Add( U _sound, AudioClip _clip )
        {
            datas.Add( _sound, _clip );
        }
    }

    private AudioSource source;
    private Dictionary<T, SoundInfo> sounds = new Dictionary<T, SoundInfo>();

    public SoundInfo this[T type] => sounds.ContainsKey( type ) ? sounds[type] : null;

    public float Volume
    {
        get => source.volume;
        set => source.volume = value;
    }

    public SoundGroup( AudioSource _source )
    {
        source = _source;
    }

    public void Add( T _type, U _soundType, AudioClip _clip )
    {
        if ( !sounds.ContainsKey( _type ) )
            sounds.Add( _type, new SoundInfo() );

        sounds[_type].Add( _soundType, _clip );
    }

    public void Play( T _type, U _soundType )
    {
        if ( !sounds.ContainsKey( _type ) || sounds[_type] == null )
        {
            Debug.LogWarning( $"{_type} is not registered" );
            return;
        }

        AudioClip clip = sounds[_type][_soundType];
        if ( clip == null )
        {
            Debug.LogWarning( $"{_soundType} is not registered" );
            return;
        }

        source.volume = 0f;
        source.clip = clip;
        source.Play();
    }

    public void PlayOneShot( T _type, U _soundType )
    {
        if ( !sounds.ContainsKey( _type ) || sounds[_type] == null )
        {
            Debug.LogWarning( $"{_type} is not registered" );
            return;
        }

        AudioClip clip = sounds[_type][_soundType];
        if ( clip == null )
        {
            Debug.LogWarning( $"{_soundType} is not registered" );
            return;
        }

        source.PlayOneShot( clip );
    }
}

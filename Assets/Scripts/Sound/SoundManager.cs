using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent( typeof( AudioSource ) )]
public class SoundManager : Singleton<SoundManager>
{
    public enum SoundType : byte 
    {
        // Main BGM
        Lobby, 
        InGame, 

        // Buttons
        MouseClick,
        MouseHover,

        MenuEntry,
        MenuExit,

        // InGame Actor Sounds
        Attack, 
        Hit, 
        Dead, 
    }

    [System.Serializable]
    public struct Sound
    {
        public SoundType type;
        public AudioClip clip;
    }

    public SoundData datas;
    private AudioSource channel;
    private Dictionary<SoundType, AudioClip> sounds = new Dictionary<SoundType, AudioClip>();

    protected override void Awake()
    {
        base.Awake();

        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "AudioSource is not found" );

        if ( datas == null )
             Debug.LogError( "Sound Data is null" );

        for ( int i = 0; i < datas.sounds.Count; i++ )
        {
            sounds[datas.sounds[i].type] = datas.sounds[i].clip;
        }
    }

    public void Play( SoundType _type )
    {
        if ( !sounds.ContainsKey( _type ) || sounds[_type] == null )
        {
            Debug.LogWarning( $"{_type} is not registered" );
            return;
        }

        channel.PlayOneShot( sounds[_type] );
    }

    public void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
        {
            Play( SoundType.Hit );
        }
        else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
        {
            Play( SoundType.Attack );
        }
        else if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
        {
            Play( SoundType.Dead );
        }
    }
}

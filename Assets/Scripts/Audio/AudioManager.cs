using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Audio;


public class AudioManager : Singleton<AudioManager>
{
    private class AudioClipGroup<T, U> where T : System.Enum where U : System.Enum
    {
        public class ClipGroup
        {
            private Dictionary<U, AudioClip> datas = new Dictionary<U, AudioClip>();

            public AudioClip this[U type] => datas.ContainsKey( type ) ? datas[type] : null;

            public void Add( U _sound, AudioClip _clip )
            {
                datas.Add( _sound, _clip );
            }
        }

        private Dictionary<T, ClipGroup> sounds = new Dictionary<T, ClipGroup>();

        public bool TryGetClip( out AudioClip _clip, T _type, U _soundType )
        {
            _clip = null;
            if ( !sounds.ContainsKey( _type ) || sounds[_type] == null )
            {
                Debug.LogWarning( $"{_type} is not registered" );
                return false;
            }

            AudioClip clip = sounds[_type][_soundType];
            if ( clip == null )
            {
                Debug.LogWarning( $"{_soundType} is not registered" );
                return false;
            }

            _clip = clip;
            return true;
        }

        public void Add( T _type, U _soundType, AudioClip _clip )
        {
            if ( !sounds.ContainsKey( _type ) )
                sounds.Add( _type, new ClipGroup() );

            sounds[_type].Add( _soundType, _clip );
        }
    }

    private AudioMixer mixer;
    private WNS.ObjectPool<AudioChannel> channels;
    private AudioClipGroup<ThemeType,  ThemeSound>  themeClips  = new AudioClipGroup<ThemeType,  ThemeSound>();
    private AudioClipGroup<PlayerType, PlayerSound> playerClips = new AudioClipGroup<PlayerType, PlayerSound>();
    // sfx.. misc.. 

    [Header( "Addressable" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();

        LoadAssetsAsync( "Audio Mixer",  ( AudioMixer _data ) => mixer = _data );
        LoadAssetsAsync( "Audio Prefab", ( GameObject _data ) => 
        {
            if ( _data.TryGetComponent( out AudioChannel channel ) )
                 channels = new WNS.ObjectPool<AudioChannel>( channel, transform );
        } );

        LoadAssetsAsync( "Theme_Default", ( AudioDataTheme _data ) => 
        {
            themeClips.Add( ThemeType.Default, ThemeSound.Login,  _data.Login  );
            themeClips.Add( ThemeType.Default, ThemeSound.Lobby,  _data.Lobby  );
            themeClips.Add( ThemeType.Default, ThemeSound.InGame, _data.InGame );
            
            themeClips.Add( ThemeType.Default, ThemeSound.MouseClick, _data.MouseClick );
            themeClips.Add( ThemeType.Default, ThemeSound.MouseHover, _data.MouseHover );
            themeClips.Add( ThemeType.Default, ThemeSound.MenuEntry,  _data.MenuEntry  );
            themeClips.Add( ThemeType.Default, ThemeSound.MenuExit,   _data.MenuExit   );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataPlayer _data ) =>
        {
            playerClips.Add( PlayerType.Default, PlayerSound.Attack, _data.Attack );
            playerClips.Add( PlayerType.Default, PlayerSound.Dead,   _data.Dead   );
            playerClips.Add( PlayerType.Default, PlayerSound.Hit,    _data.Hit    );
        } );
    }

    public void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
        {
            Play( ThemeType.Default, ThemeSound.Login );
        }
    }

    private void OnDestroy()
    {
        foreach ( var handle in handles )
        {
            if ( !handle.IsDone )
                 Debug.LogWarning( $"The {handle.DebugName} operation is in progress" );

            Addressables.Release( handle );
        }

        handles.Clear();
    }
    #endregion

    #region Play
    private AudioChannel GetChannel<T, U>( in AudioClipGroup<T, U> _clips, T _type, U _sound, float _volume ) where T : System.Enum where U : System.Enum
    {
        AudioChannel channel = null;
        if ( _clips.TryGetClip( out AudioClip clip, _type, _sound ) )
        {
            channel = channels.Spawn();
            channel.Clip   = clip;
            channel.Volume = _volume;
        }

        return channel;
    }

    /// <summary> Play with no effect </summary>
    public void Play( ThemeType _type, ThemeSound _sound, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( themeClips, _type, _sound, _volume );
        channel.Play();
    }

    /// <summary> Play with fade effect </summary>
    public void Play( ThemeType _type, ThemeSound _sound, float _start, float _end, float _t )
    {
        AudioChannel channel = GetChannel( themeClips, _type, _sound, 0f );
        StartCoroutine( Fade( channel, _start, _end, _t ) );
    }

    /// <summary> Play with no effect </summary>
    public void Play( PlayerType _type, PlayerSound _sound, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( playerClips, _type, _sound, _volume );
        channel.Play();
    }

    /// <summary> Play the sound at the _point </summary>
    public void Play( PlayerType _type, PlayerSound _sound, Vector3 _point, float _volume = 1f )
    {
        if ( playerClips.TryGetClip( out AudioClip clip, _type, _sound ) )
             AudioSource.PlayClipAtPoint( clip, _point, _volume );
    }
    #endregion

    #region Effect
    private IEnumerator Fade( AudioChannel _channel, float _start, float _end, float _t )
    {
        if ( Global.Mathematics.Abs( _start - _end ) < float.Epsilon )
        {
            _channel.Volume = _end;
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        _channel.Volume = _start;
        _channel.Play();
        while ( _start < _end ? elapsedVolume < _end : // FADEIN
                                elapsedVolume > _end ) // FADEOUT
        {
            yield return YieldCache.WaitForEndOfFrame;
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            _channel.Volume = elapsedVolume;
        }
        _channel.Volume = _end;
    }
    #endregion

    #region Addressable
    private void LoadAssetsAsync<T>( string _label, System.Action<T> _OnCompleted ) where T : UnityEngine.Object
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync( _label, typeof( T ) );
        handles.Add( locationHandle );

        locationHandle.Completed += ( AsyncOperationHandle<IList<IResourceLocation>> _handle ) => 
        {
            if ( _handle.Status != AsyncOperationStatus.Succeeded )
            {
                Debug.LogWarning( "Load Location Async Failed" );
                return;
            }

            foreach ( IResourceLocation location in _handle.Result )
            {
                AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>( location );
                handles.Add( assetHandle );

                assetHandle.Completed += ( AsyncOperationHandle<T> _handle ) =>
                {
                    if ( _handle.Status != AsyncOperationStatus.Succeeded )
                    {
                        Debug.LogError( "Load Asset Async Failed" );
                        return;
                    }

                    _OnCompleted?.Invoke( _handle.Result );
                };
            }
        };
    }
    #endregion
}


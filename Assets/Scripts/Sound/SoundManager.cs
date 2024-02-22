using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Audio;


public class SoundManager : Singleton<SoundManager>
{
    public class AudioClipGroup<T, U> where T : System.Enum where U : System.Enum
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
        private ClipGroup this[T type] => sounds.ContainsKey( type ) ? sounds[type] : null;

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
    public void Play( ThemeType _type, ThemeSound _sound )
    {
        Debug.Log( mixer.outputAudioMixerGroup.name );
        // if ( themeClips.TryGetClip( out AudioClip clip, _type, _sound ) )
        // {
        //     var channel = channels.Spawn();
        // 
        // 
        // }
    }

    //public void PlayOneShot( ThemeType _type, ThemeSound _sound )
    //{
    //    themeGroup.PlayOneShot( _type, _sound );
    //}

    //public void Play( PlayerType _type, PlayerSound _sound )
    //{
    //    playerGroup.Play( _type, _sound );
    //}

    //public void PlayOneShot( PlayerType _type, PlayerSound _sound )
    //{
    //    playerGroup.Play( _type, _sound );
    //}
    #endregion

    #region Addressable

    //private void LoadAssetsAsync<T>( string _lable, System.Action<T> _OnCompleted ) where T : Object
    //             => StartCoroutine( LoadAssetsAsyncCoroutine<T>( _lable, _OnCompleted ) );

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


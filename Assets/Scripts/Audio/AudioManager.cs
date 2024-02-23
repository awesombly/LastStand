using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Audio;


public enum MixerType : int { Master = 0, BGM, SFX, }
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
    private AudioMixerGroup[] mixerGroup;
    private WNS.ObjectPool<AudioChannel> channels;
    private LinkedList<AudioChannel> enabledChannels = new LinkedList<AudioChannel>();
    private AudioClipGroup<BGMType,    BGMSound>    bgmClips    = new AudioClipGroup<BGMType,    BGMSound>();
    private AudioClipGroup<SFXType,    SFXSound>    sfxClips    = new AudioClipGroup<SFXType,    SFXSound>();
    private AudioClipGroup<PlayerType, PlayerSound> playerClips = new AudioClipGroup<PlayerType, PlayerSound>();
    private AudioClipGroup<WeaponType, WeaponSound> weaponClips = new AudioClipGroup<WeaponType, WeaponSound>();

    [Header( "Addressable" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
    private int totalCount, loadCount;
    public bool IsLoading { get; private set; } = true;

    public void Despawn( AudioChannel _channel )
    {
        enabledChannels.Remove( _channel );
        channels.Despawn( _channel );
    }

    public void AllStop()
    {
        foreach ( var channel in enabledChannels )
            channels.Despawn( channel );

        enabledChannels.Clear();
    }

    public void MixerDecibelControl( MixerType _type, float _volume )
    {
        string groupName = _type == MixerType.BGM ? "BGM" :
                           _type == MixerType.SFX ? "SFX" : "Master";

        float volume = ( 60f * _volume ) - 60f;
        mixer.SetFloat( groupName, volume < -60f ? -80f : volume );
    }

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();

        SceneBase.OnBeforeSceneLoad += AllStop;

        LoadAssetsAsync( "Audio_Mixer",  ( AudioMixer _data ) => 
        {
            mixer = _data;
            mixerGroup = mixer.FindMatchingGroups( "Master" );
        } );

        LoadAssetsAsync( "Audio_Prefab", ( GameObject _data ) => 
        {
            if ( _data.TryGetComponent( out AudioChannel _channel ) )
                 channels = new WNS.ObjectPool<AudioChannel>( _channel, transform );
        } );

        LoadAssetsAsync( "Theme_Default", ( AudioDataBGM _data ) =>
        {
            bgmClips.Add( BGMType.Default, BGMSound.Login,  _data.Login );
            bgmClips.Add( BGMType.Default, BGMSound.Lobby,  _data.Lobby );
            bgmClips.Add( BGMType.Default, BGMSound.InGame, _data.InGame );
        } );

        LoadAssetsAsync( "Theme_Default", ( AudioDataSFX _data ) => 
        {
            sfxClips.Add( SFXType.Default, SFXSound.MouseClick, _data.MouseClick );
            sfxClips.Add( SFXType.Default, SFXSound.MenuEntry,  _data.MenuEntry  );
            sfxClips.Add( SFXType.Default, SFXSound.MenuExit,   _data.MenuExit   );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataPlayer _data ) =>
        {
            playerClips.Add( PlayerType.Default, PlayerSound.Attack, _data.Attack );
            playerClips.Add( PlayerType.Default, PlayerSound.Dead,   _data.Dead   );
            playerClips.Add( PlayerType.Default, PlayerSound.Hit,    _data.Hit    );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.Default, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.Default, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.Default, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.Default, WeaponSound.Swap, _data.Swap );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.Handgun, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.Handgun, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.Handgun, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.Handgun, WeaponSound.Swap, _data.Swap );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.Rifle, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.Rifle, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.Rifle, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.Rifle, WeaponSound.Swap, _data.Swap );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.BurstRifle, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.BurstRifle, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.BurstRifle, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.BurstRifle, WeaponSound.Swap, _data.Swap );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.Shotgun, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.Shotgun, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.Shotgun, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.Shotgun, WeaponSound.Swap, _data.Swap );
        } );

        LoadAssetsAsync( "Player_Default", ( AudioDataWeapon _data ) =>
        {
            weaponClips.Add( WeaponType.AssaultShotgun, WeaponSound.Fire, _data.Fire );
            weaponClips.Add( WeaponType.AssaultShotgun, WeaponSound.Hit, _data.Hit );
            weaponClips.Add( WeaponType.AssaultShotgun, WeaponSound.Reload, _data.Reload );
            weaponClips.Add( WeaponType.AssaultShotgun, WeaponSound.Swap, _data.Swap );
        } );

        StartCoroutine( CheckLoadCount() );
    }

    private IEnumerator CheckLoadCount()
    {
        yield return new WaitUntil( () => totalCount > 0 && totalCount == loadCount );
        IsLoading = false;
    }

    private void OnDestroy()
    {
        AllStop();
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
    private AudioChannel GetChannel<T, U>( AudioClipGroup<T, U> _clips, T _type, U _sound, float _volume ) where T : System.Enum where U : System.Enum
    {
        AudioChannel channel = null;
        if ( _clips.TryGetClip( out AudioClip clip, _type, _sound ) )
        {
            channel = channels.Spawn();
            channel.Clip   = clip;
            channel.Volume = _volume;

            enabledChannels.AddLast( channel );
        }

        return channel;
    }

    #region BGM
    /// <summary> Play with no effect </summary>
    public void Play( BGMType _type, BGMSound _sound, float _volume = 1f, bool _loop = true )
    {
        AudioChannel channel = GetChannel( bgmClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.BGM];
        channel.Loop = _loop;
        channel.Play();
    }

    /// <summary> Play with fade effect </summary>
    public void Play( BGMType _type, BGMSound _sound, float _start, float _end, float _t, bool _loop = true )
    {
        AudioChannel channel = GetChannel( bgmClips, _type, _sound, 0f );
        channel.MixerGroup = mixerGroup[( int )MixerType.BGM];
        channel.Loop = _loop;
        StartCoroutine( Fade( channel, _start, _end, _t ) );
    }
    #endregion

    #region SFX
    /// <summary> Play with no effect </summary>
    public void Play( SFXType _type, SFXSound _sound, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( sfxClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.Play();
    }
    #endregion

    #region Character
    /// <summary> Play with no effect </summary>
    public void Play( PlayerType _type, PlayerSound _sound, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( playerClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.Play();
    }

    /// <summary> Play the sound at the _point </summary>
    public void Play( PlayerType _type, PlayerSound _sound, Vector3 _point, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( playerClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.transform.position = _point;
        channel.Play();
    }
    #endregion
    #region Character
    /// <summary> Play with no effect </summary>
    public void Play( WeaponType _type, WeaponSound _sound, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( weaponClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.Play();
    }

    /// <summary> Play the sound at the _point </summary>
    public void Play( WeaponType _type, WeaponSound _sound, Vector3 _point, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( weaponClips, _type, _sound, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.transform.position = _point;
        channel.Play();
    }
    #endregion
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

            totalCount += _handle.Result.Count;
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
                    loadCount++;
                };
            }
        };
    }
    #endregion
}


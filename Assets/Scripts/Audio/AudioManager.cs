using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Audio;


public enum MixerType : int { Master = 0, BGM, SFX, }
public enum BGM : ushort
{
    Login,
    Lobby,
    InGame,
}

public enum SFX : ushort
{
    MouseClick,
    MenuEntry,
    MenuExit,
}

public class AudioManager : Singleton<AudioManager>
{
    public class AudioClipGroup<T> where T : System.Enum
    {
        private Dictionary<T, AudioClip> datas = new Dictionary<T, AudioClip>();

        public AudioClip this[T _type] => TryGetClip( out AudioClip _clip, _type ) ? _clip : null;
        
        public bool TryGetClip( out AudioClip _clip, T _type )
        {
            _clip = null;
            if ( !datas.ContainsKey( _type ) || datas[_type] == null )
            {
                Debug.LogWarning( $"{_type} is not registered" );
                return false;
            }

            _clip = datas[_type];
            return true;
        }

        public void Add( T _sound, AudioClip _clip )
        {
            datas.Add( _sound, _clip );
        }
    }
    private AudioMixer mixer;
    private AudioMixerGroup[] mixerGroup;
    private WNS.ObjectPool<AudioChannel> channels;
    private LinkedList<AudioChannel> enabledChannels = new LinkedList<AudioChannel>();
    private AudioClipGroup<BGM> bgmClips = new AudioClipGroup<BGM>();
    private AudioClipGroup<SFX> sfxClips = new AudioClipGroup<SFX>();

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

        LoadAssetsAsync( "Global", ( GlobalAudioData _data ) =>
        {
            mixer = _data.mixer;
            mixerGroup = mixer.FindMatchingGroups( "Master" );

            channels = new WNS.ObjectPool<AudioChannel>( _data.channel, transform );

            bgmClips.Add( BGM.Login,  _data.bgmLogin  );
            bgmClips.Add( BGM.Lobby,  _data.bgmLobby  );
            bgmClips.Add( BGM.InGame, _data.bgmInGame );
                          
            sfxClips.Add( SFX.MouseClick, _data.sfxMouseClick );
            sfxClips.Add( SFX.MenuEntry,  _data.sfxMenuEntry  );
            sfxClips.Add( SFX.MenuExit,   _data.sfxMenuExit   );
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
    private AudioChannel GetChannel<T>( AudioClipGroup<T> _clips, T _type, float _volume ) where T : System.Enum
    {
        AudioChannel channel = null;
        if ( _clips.TryGetClip( out AudioClip clip, _type ) )
        {
            channel = channels.Spawn();
            channel.Clip = clip;
            channel.Volume = _volume;

            enabledChannels.AddLast( channel );
        }

        return channel;
    }

    #region BGM
    /// <summary> Play with fade effect </summary>
    public void Play( BGM _type, float _start, float _end, float _t, bool _loop = true )
    {
        AudioChannel channel = GetChannel( bgmClips, _type, 0f );
        channel.MixerGroup = mixerGroup[( int )MixerType.BGM];
        channel.Loop = _loop;
        StartCoroutine( Fade( channel, _start, _end, _t ) );
    }
    #endregion

    #region SFX
    /// <summary> Play with no effect </summary>
    public void Play( SFX _type, float _volume = 1f )
    {
        AudioChannel channel = GetChannel( sfxClips, _type, _volume );
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.Play();
    }
    #endregion

    public void Play( in AudioClip _clip, float _volume = 1f )
    {
        AudioChannel channel = channels.Spawn();
        channel.MixerGroup   = mixerGroup[( int )MixerType.SFX];
        channel.Clip         = _clip;
        channel.Volume       = _volume;
        channel.Play();

        enabledChannels.AddLast( channel );
    }

    public void Play( in AudioClip _clip, Vector3 _pos, float _volume = 1f )
    {
        AudioChannel channel = channels.Spawn();
        channel.MixerGroup = mixerGroup[( int )MixerType.SFX];
        channel.Clip = _clip;
        channel.Volume = _volume;
        channel.transform.position = _pos;
        
        channel.Play();
        enabledChannels.AddLast( channel );
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


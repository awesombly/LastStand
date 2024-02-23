using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Audio;
using System.Runtime.Remoting.Channels;
using Unity.VisualScripting;
using System.Runtime.InteropServices;


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
    public class AudioClipGroup
    {
        private Dictionary<System.Enum, AudioClip> datas = new Dictionary<System.Enum, AudioClip>();

        public AudioClip this[System.Enum _type] => TryGetClip( out AudioClip _clip, _type ) ? _clip : null;

        public bool TryGetClip<T>( out AudioClip _clip, T _type ) where T : System.Enum
        {
            if ( !datas.ContainsKey( _type ) || datas[_type] == null )
            {
                Debug.LogWarning( $"Could not import {_type}" );
                _clip = null;
                return false;
            }

            _clip = datas[_type];
            return true;
        }

        public void Add<T>( T _type, AudioClip _clip ) where T : System.Enum
        {
            if ( datas.ContainsKey( _type ) || _clip == null )
            {
                Debug.LogWarning( $"Unable to register {_type}" );
                return;
            }

            datas.Add( _type, _clip );
        }
    }

    #region Variables
    private AudioMixer mixer;
    private AudioMixerGroup[] mixerGroup;
    private WNS.ObjectPool<AudioChannel> channels;
    private AudioClipGroup clips = new AudioClipGroup();

    [Header( "Addressable" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
    private int totalCount, loadCount;
    public bool IsLoading { get; private set; } = true;
    #endregion

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();

        SceneBase.OnBeforeSceneLoad += AllStop;
        LoadAssetsAsync( "Global", ( GlobalAudioData _data ) =>
        {
            mixer = _data.mixer;
            mixerGroup = mixer?.FindMatchingGroups( "Master" );

            if ( !ReferenceEquals( _data.channel, null ) )
                 channels = new WNS.ObjectPool<AudioChannel>( _data.channel, transform );

            clips.Add( BGM.Login,  _data.bgmLogin  );
            clips.Add( BGM.Lobby,  _data.bgmLobby  );
            clips.Add( BGM.InGame, _data.bgmInGame );

            clips.Add( SFX.MouseClick, _data.sfxMouseClick );
            clips.Add( SFX.MenuEntry,  _data.sfxMenuEntry  );
            clips.Add( SFX.MenuExit,   _data.sfxMenuExit   );
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
    private AudioChannel GetChannel( AudioClip _clip, MixerType _type, float _volume, bool _loop )
    {
        AudioChannel channel = channels.Spawn();
        channel.MixerGroup = mixerGroup[( int )_type];
        channel.Clip   = _clip;
        channel.Volume = _volume;
        channel.Loop   = _loop;
        return channel;
    }

    /// <summary> Play with fade effect </summary>
    public void Play( BGM _type, float _start, float _end, float _t, bool _loop = true )
    {
        if ( clips.TryGetClip( out AudioClip clip, _type ) )
        {
            AudioChannel channel = GetChannel( clip, MixerType.BGM, 0f, _loop );
            StartCoroutine( Fade( channel, _start, _end, _t ) );
        }
    }

    /// <summary> Play with no effect </summary>
    public void Play( SFX _type, float _volume = 1f )
    {
        if ( clips.TryGetClip( out AudioClip clip, _type ) )
        {
            AudioChannel channel = GetChannel( clip, MixerType.SFX, _volume, false );
            channel.Play();
        }
    }

    public void Play( AudioClip _clip, float _volume = 1f )
    {
        if ( !ReferenceEquals( _clip, null ) )
        {
            AudioChannel channel = GetChannel( _clip, MixerType.SFX, _volume, false );
            channel.Play();
        }
    }

    public void Play( AudioClip _clip, Vector3 _pos, float _volume = 1f )
    {
        if ( !ReferenceEquals( _clip, null ) )
        {
            AudioChannel channel = GetChannel( _clip, MixerType.SFX, _volume, false );
            channel.transform.position = _pos;
            channel.Play();
        }
    }
    #endregion

    #region Stop
    public void Despawn( AudioChannel _channel ) => channels?.Despawn( _channel );

    public void AllStop() => channels?.AllDespawn();
    #endregion

    #region AudioMixer
    public void MixerDecibelControl( MixerType _type, float _volume )
    {
        string groupName = _type == MixerType.BGM ? "BGM" :
                           _type == MixerType.SFX ? "SFX" : "Master";

        float volume = ( 60f * _volume ) - 60f;
        mixer?.SetFloat( groupName, volume < -60f ? -80f : volume );
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


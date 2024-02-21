using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

using static InterfaceSoundScriptable;
using static PlayerSoundScriptable;

[RequireComponent( typeof( AudioSource ) )]
public class SoundManager : Singleton<SoundManager>
{
    private class SoundInfo<T> where T : System.Enum
    {
        private Dictionary<T, AudioClip> datas = new Dictionary<T, AudioClip>();

        public AudioClip this[T type] => datas.ContainsKey( type ) ? datas[type] : null;

        public void Add( T _sound, AudioClip _clip )
        {
            datas.Add( _sound, _clip );
        }
    }

    private AudioSource channel;
    private Dictionary<InterfaceType, SoundInfo<InterfaceSound>> interfaceSounds = new Dictionary<InterfaceType, SoundInfo<InterfaceSound>>();
    private Dictionary<PlayerType,    SoundInfo<PlayerSound>>    playerSounds    = new Dictionary<PlayerType,    SoundInfo<PlayerSound>>();
    // sfx.. misc.. 

    [Header( "Addressables" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
    private int totalCount = 0;
    private int loadCount  = 0;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();

        if ( !TryGetComponent( out channel ) )
             Debug.LogError( "AudioSource is not found" );

        LoadAssetsAsync<InterfaceSoundScriptable>( "Sound_Interface", ( InterfaceSoundScriptable _data ) => 
        {
            if ( !interfaceSounds.ContainsKey( _data.type ) )
                 interfaceSounds.Add( _data.type, new SoundInfo<InterfaceSound>() );

            foreach ( var data in _data.datas )
            {
                interfaceSounds[_data.type].Add( data.soundType, data.clip );
            }
        } );

        LoadAssetsAsync<PlayerSoundScriptable>( "Sound_Player", ( PlayerSoundScriptable _data ) => 
        {
            if ( !playerSounds.ContainsKey( _data.type ) )
                 playerSounds.Add( _data.type, new SoundInfo<PlayerSound>() );

            foreach ( var data in _data.datas )
            {
                playerSounds[_data.type].Add( data.soundType, data.clip );
            }
        } );

        StartCoroutine( ClearHandle() );
    }

    public void Update()
    {
        //if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
        //{
        //    Play( PlayerSound.Attack );
        //}
        //else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
        //{
        //    Play( PlayerSound.Dead );
        //}
        //else if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
        //{
        //    Play( InterfaceSound.Login );
        //}
    }
    #endregion

    #region Play
    public void Play( InterfaceSound _sound, InterfaceType _type = InterfaceType.Default )
    {
        if ( !interfaceSounds.ContainsKey( _type ) || interfaceSounds[_type] == null )
        {
            Debug.LogWarning( $"{_type} is not registered" );
            return;
        }

        AudioClip clip = interfaceSounds[_type][_sound];
        if ( clip == null )
        {
            Debug.LogWarning( $"{_sound} is not registered" );
            return;
        }

        channel.PlayOneShot( clip );
    }

    public void Play( PlayerSound _sound, PlayerType _type = PlayerType.Default )
    {
        if ( !playerSounds.ContainsKey( _type ) || playerSounds[_type] == null )
        {
            Debug.LogWarning( $"{_type} is not registered" );
            return;
        }

        AudioClip clip = playerSounds[_type][_sound];
        if ( clip == null )
        {
            Debug.LogWarning( $"{_sound} is not registered" );
            return;
        }

        channel.PlayOneShot( clip );
    }
    #endregion

    #region Addressable
    private IEnumerator ClearHandle()
    {
        yield return YieldCache.WaitForSeconds( 5f );
        yield return new WaitUntil( () => totalCount == loadCount );
        foreach ( var handle in handles )
        {
            if ( !handle.IsDone )
                 Debug.LogWarning( $"The {handle.DebugName} operation is in progress" );

            Addressables.Release( handle );
        }

        handles.Clear();
    }

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
                    loadCount++;
                    if ( _handle.Status != AsyncOperationStatus.Succeeded )
                    {
                        Debug.LogError( "Load Asset Async Failed" );
                        return;
                    }

                    _OnCompleted?.Invoke( _handle.Result );
                };
            }
        };

        totalCount += locationHandle.Result.Count;
    }
    #endregion
}


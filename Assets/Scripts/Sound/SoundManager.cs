using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class SoundManager : Singleton<SoundManager>
{
    private SoundGroup<ThemeType,  ThemeSound>  themeGroup;
    private SoundGroup<PlayerType, PlayerSound> playerGroup;
    // sfx.. misc.. 

    [Header( "Addressables" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

    private void CreateChannelGroup<T, U>( out SoundGroup<T, U> _group, string _ObjName ) where T : System.Enum where U : System.Enum
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale    = Vector3.one;
        obj.name = _ObjName;

        // 기본 설정
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;

        _group = new SoundGroup<T, U>( source );
    }

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();

        CreateChannelGroup( out themeGroup, "Theme Group" );
        LoadAssetsAsync<ThemeSoundScriptable>( "Sound_Theme", ( ThemeSoundScriptable _data ) => 
        {
            foreach ( var data in _data.datas )
                themeGroup.Add( _data.type, data.soundType, data.clip );
        } );

        CreateChannelGroup( out playerGroup, "Player Group" );
        LoadAssetsAsync<PlayerSoundScriptable>( "Sound_Player", ( PlayerSoundScriptable _data ) => 
        {
            foreach ( var data in _data.datas )
                playerGroup.Add( _data.type, data.soundType, data.clip );
        } );
    }

    public void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
        {
            Play( ThemeType.Default, ThemeSound.Login );
        }
        else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
        {
            PlayOneShot( ThemeType.Default, ThemeSound.Lobby );
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
        themeGroup.Play( _type, _sound );
    }

    public void PlayOneShot( ThemeType _type, ThemeSound _sound )
    {
        themeGroup.PlayOneShot( _type, _sound );
    }

    public void Play( PlayerType _type, PlayerSound _sound )
    {
        playerGroup.Play( _type, _sound );
    }

    public void PlayOneShot( PlayerType _type, PlayerSound _sound )
    {
        playerGroup.Play( _type, _sound );
    }
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


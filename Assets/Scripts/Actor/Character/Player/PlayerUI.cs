using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [HideInInspector]
    public Global.StatusFloat respawnDelay;

    public Slider reloadBar;
    private Player player;
    private InGameUIScene uiScene;

    private void Awake()
    {
        uiScene = GameManager.Inst.GetActiveScene( SceneType.InGame_UI ) as InGameUIScene;
        if ( uiScene is null )
        {
            Debug.LogError( "Not found UIScene." );
        }

        reloadBar.gameObject.SetActive( false );
        player = GetComponent<Player>();
        player.OnChangeEquipWeapon += OnChangeEquipWeapon;
        OnChangeEquipWeapon( null, player.EquipWeapon );
    }

    private void OnChangeAmmo( int _old, int _new, int _max )
    {
        if ( !ReferenceEquals( player, null ) && player.IsLocal )
        {
            uiScene.ammoText?.SetText( _new.ToString() );
        }
    }

    private void OnChangeMagazine( int _old, int _new, int _max )
    {
        if ( !ReferenceEquals( player, null ) && player.IsLocal )
        {
            uiScene.magazineText?.SetText( _new.ToString() );
        }
    }

    private void OnChangeReloadDelay( float _old, float _new, float _max )
    {
        // swapDelay(무기교체시)도 같이 사용
        bool isActive = !( player.EquipWeapon.myStat.reloadDelay.IsZero && player.EquipWeapon.myStat.swapDelay.IsZero );
        reloadBar.gameObject.SetActive( isActive );

        if ( player.EquipWeapon.myStat.reloadDelay.Current >= player.EquipWeapon.myStat.swapDelay.Current )
        {
            reloadBar.value = ( player.EquipWeapon.myStat.reloadDelay.Max - player.EquipWeapon.myStat.reloadDelay.Current ) / player.EquipWeapon.myStat.reloadDelay.Max;
        }
        else
        {
            reloadBar.value = ( player.EquipWeapon.myStat.swapDelay.Max - player.EquipWeapon.myStat.swapDelay.Current ) / player.EquipWeapon.myStat.swapDelay.Max;
        }
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old is not null )
        {
            _old.myStat.ammo.OnChangeCurrent           -= OnChangeAmmo;
            _old.myStat.magazine.OnChangeCurrent       -= OnChangeMagazine;
            _old.myStat.reloadDelay.OnChangeCurrent    -= OnChangeReloadDelay;
            _old.myStat.swapDelay.OnChangeCurrent      -= OnChangeReloadDelay;
        }

        if ( _new is not null )
        {
            _new.myStat.ammo.OnChangeCurrent           += OnChangeAmmo;
            _new.myStat.magazine.OnChangeCurrent       += OnChangeMagazine;
            _new.myStat.reloadDelay.OnChangeCurrent    += OnChangeReloadDelay;
            _new.myStat.swapDelay.OnChangeCurrent      += OnChangeReloadDelay;

            OnChangeAmmo( 0, _new.myStat.ammo.Current, _new.myStat.ammo.Max );
            OnChangeMagazine( 0, _new.myStat.magazine.Current, _new.myStat.magazine.Max );
            OnChangeReloadDelay( 0f, _new.myStat.reloadDelay.Current, _new.myStat.reloadDelay.Max );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider reloadBar;
    private Player player;

    private void Awake()
    {
        reloadBar.gameObject.SetActive( false );
        player = GetComponent<Player>();
        player.OnChangeEquipWeapon += OnChangeEquipWeapon;
        OnChangeEquipWeapon( null, player.EquipWeapon );
    }

    private void OnChangeAmmo( int _old, int _new )
    {
        if ( !ReferenceEquals( player, null ) && player.IsLocal )
        {
            UIManager.Inst.ammoText?.SetText( _new.ToString() );
        }
    }

    private void OnChangeMagazine( int _old, int _new )
    {
        if ( !ReferenceEquals( player, null ) && player.IsLocal )
        {
            UIManager.Inst.magazineText?.SetText( _new.ToString() );
        }
    }

    private void OnChangeReloadDelay( float _old, float _new )
    {
        reloadBar.gameObject.SetActive( !player.EquipWeapon.stat.reloadDelay.IsZero );
        reloadBar.value = ( player.EquipWeapon.stat.reloadDelay.Max - player.EquipWeapon.stat.reloadDelay.Current ) / player.EquipWeapon.stat.reloadDelay.Max;
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old != null )
        {
            _old.stat.ammo.OnChangeCurrent           -= OnChangeAmmo;
            _old.stat.magazine.OnChangeCurrent       -= OnChangeMagazine;
            _old.stat.reloadDelay.OnChangeCurrent    -= OnChangeReloadDelay;
        }

        if ( _new != null )
        {
            _new.stat.ammo.OnChangeCurrent           += OnChangeAmmo;
            _new.stat.magazine.OnChangeCurrent       += OnChangeMagazine;
            _new.stat.reloadDelay.OnChangeCurrent    += OnChangeReloadDelay;

            OnChangeAmmo( 0, _new.stat.ammo.Current );
            OnChangeMagazine( 0, _new.stat.magazine.Current );
            OnChangeReloadDelay( 0f, _new.stat.reloadDelay.Current );
        }
    }
}

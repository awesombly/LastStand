using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatUi : MonoBehaviour
{
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;
    public Slider reloadBar;

    private Weapon equipWeapon;

    private void Awake()
    {
        reloadBar.gameObject.SetActive( false );
        
        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;
    }

    private void OnChangeAmmo( int _old, int _new )
    {
        ammoText?.SetText( _new.ToString() );
    }

    private void OnChangeMagazine( int _old, int _new )
    {
        magazineText?.SetText( _new.ToString() );
    }

    private void OnChangeReloadDelay( float _old, float _new )
    {
        reloadBar.gameObject.SetActive( !equipWeapon.stat.reloadDelay.IsZero );
        reloadBar.value = ( equipWeapon.stat.reloadDelay.Max - equipWeapon.stat.reloadDelay.Current ) / equipWeapon.stat.reloadDelay.Max;
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        equipWeapon = _new;
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
        else
        {
            OnChangeAmmo( 0, 0 );
            OnChangeMagazine( 0, 0 );
        }
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        Weapon oldWeapon = null;
        Weapon newWeapon = null;
        if ( _old != null )
        {
            oldWeapon = _old.EquipWeapon;
            _old.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }

        if ( _new != null )
        {
            newWeapon = _new.EquipWeapon;
            _new.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }

        OnChangeEquipWeapon( oldWeapon, newWeapon );

        /// TODO: reloadBar 교체 필요
    }
}

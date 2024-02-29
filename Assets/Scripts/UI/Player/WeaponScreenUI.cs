using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponScreenUI : MonoBehaviour
{
    [SerializeField]
    private Image weaponImage;

    private void Awake()
    {
        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old != null )
        {
            _old.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }

        if ( _new != null )
        {
            _new.OnChangeEquipWeapon += OnChangeEquipWeapon;
        }
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new || ReferenceEquals( _new, null ) )
        {
            return;
        }

        weaponImage.sprite = _new.GetComponent<SpriteRenderer>().sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSoundEffect : MonoBehaviour
{
    private void Awake()
    {
        Weapon weapon = GetComponent<Weapon>();
        weapon.OnFire += OnFire;
        weapon.OnReload += OnReload;
        weapon.OnSwap += OnSwap;
    }

    private void OnFire( Weapon _weapon )
    {
        AudioManager.Inst.Play( _weapon.data.weaponType, WeaponSound.Fire, _weapon.transform.position );
    }

    private void OnReload( Weapon _weapon )
    {
        AudioManager.Inst.Play( _weapon.data.weaponType, WeaponSound.Reload, _weapon.transform.position );
    }

    private void OnSwap( Weapon _weapon )
    {
        AudioManager.Inst.Play( _weapon.data.weaponType, WeaponSound.Swap, _weapon.transform.position );
    }
}

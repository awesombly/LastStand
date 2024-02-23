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
        AudioManager.Inst.Play( _weapon.data.fireSound, _weapon.transform.position );
    }

    private void OnReload( Weapon _weapon )
    {
        AudioManager.Inst.Play( _weapon.data.reloadSound, _weapon.transform.position );
    }

    private void OnSwap( Weapon _weapon )
    {
        AudioManager.Inst.Play( _weapon.data.swapSound, _weapon.transform.position );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEffect : EffectSpawner
{
    private void Awake()
    {
        Weapon weapon = GetComponent<Weapon>();
        weapon.OnFireEvent += OnFire;
        weapon.OnReloadEvent += OnReload;
    }

    private void OnFire( Weapon _weapon )
    {
        SpawnEffect( _weapon.data.fireEffect, _weapon.shotPoint );
    }

    private void OnReload( Weapon _weapon )
    {
        SpawnEffect( _weapon.data.reloadEffect, _weapon.magazinePoint );
    }
}

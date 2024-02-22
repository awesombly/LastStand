using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSound : MonoBehaviour
{
    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnHit += OnHit;
        bullet.OnFire += OnFire;
    }

    private void OnFire( Bullet _bullet )
    {
        AudioManager.Inst.Play( _bullet.data.playerType, PlayerSound.Attack, _bullet.data.volume );
    }

    private void OnHit( Character _attacker, Character _defender, Bullet _bullet )
    {
        AudioManager.Inst.Play( _bullet.data.playerType, PlayerSound.Hit, _bullet.data.volume );
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSound : MonoBehaviour
{
    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnFireEvent += OnFire;
        bullet.OnHitEvent += OnHit;
    }

    private void OnFire( Bullet _bullet )
    {
        AudioManager.Inst.Play( _bullet.data.fireSound, _bullet.transform.position );
    }

    private void OnHit( Bullet _bullet )
    {
        AudioManager.Inst.Play( _bullet.data.hitSound, _bullet.transform.position );
    }
}
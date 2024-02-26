using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSound : MonoBehaviour
{
    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnHitEvent += OnHit;
    }

    private void OnHit( Bullet _bullet )
    {
        AudioManager.Inst.Play( _bullet.data.hitSound, _bullet.transform.position );
    }
}
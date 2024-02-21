using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
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
        SoundChannel channel = SoundManager.Inst.Play( PlayerSoundScriptable.PlayerSound.Attack, _bullet.data.playerType );
        channel.Volume = _bullet.data.volume;
    }

    private void OnHit( Character _attacker, Character _defender, Bullet _bullet )
    {
        SoundChannel channel = SoundManager.Inst.Play( PlayerSoundScriptable.PlayerSound.Hit, _bullet.data.playerType );
        channel.Volume = _bullet.data.volume;
    }
}

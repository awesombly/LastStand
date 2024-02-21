using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;

public class BulletSound : MonoBehaviour
{
    [SerializeField]
    private PlayerSoundScriptable.PlayerType playerType;
    [SerializeField]
    private float volume;

    private void Awake()
    {
        Bullet bullet = GetComponent<Bullet>();
        bullet.OnHit += OnHit;
        bullet.OnFire += OnFire;
    }

    private void OnFire( Bullet _bullet )
    {
        SoundChannel channel = SoundManager.Inst.Play( PlayerSoundScriptable.PlayerSound.Attack, playerType );
        channel.Volume = volume;
    }

    private void OnHit( Character _attacker, Character _defender, Bullet _bullet )
    {
        SoundChannel channel = SoundManager.Inst.Play( PlayerSoundScriptable.PlayerSound.Hit, playerType );
        channel.Volume = volume;
    }
}

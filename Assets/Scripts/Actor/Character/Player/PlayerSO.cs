using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "PlayerSO_", menuName = "Scriptable Objects/PlayerSO" )]
public class PlayerSO : ScriptableObject
{
    public Sprite handSprite;
    public RuntimeAnimatorController playerAC;

    [Header( "< Effect >" )]
    public Poolable dodgeActionEffect;
    public Poolable dodgeHitEffect;

    [Header( "< Sound >" )]
    public AudioClip dodgeActionSound;
    public AudioClip dodgeHitSound;
}

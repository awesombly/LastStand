using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "PlayerSO_", menuName = "Scriptable Objects/PlayerSO" )]
public class PlayerSO : ScriptableObject
{
    public Sprite handSprite;
    public RuntimeAnimatorController playerAC;

    [Serializable]
    public struct DodgeInfo
    {
        public Poolable actionEffect;
        public Poolable hitEffect;

        public Poolable afeterImage;
        public float afterImageDuration;
    }
    [Header( "< Effect >" )]
    public DodgeInfo dodge;

    [Header( "< Sound >" )]
    public AudioClip dodgeActionSound;
    public AudioClip dodgeHitSound;
    public AudioClip deadSound;

    [Serializable]
    public struct VirtualPadInfo
    {
        public Sprite reloadImage;
        public Sprite dodgeImage;
        public Sprite interactionImage;
    }
    [Header( "< VirtualPad >" )]
    public VirtualPadInfo pad;
}

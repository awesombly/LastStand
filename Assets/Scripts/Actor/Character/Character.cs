using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Character : Actor
{
    [HideInInspector]
    public Global.StatusFloat Hp;
    public CharacterData data;
    [SerializeField]
    private GameObject uiCanvas;

    private bool isFlipX;
    public bool IsFlipX
    {
        get => isFlipX;
        protected set
        {
            if ( isFlipX == value )
            {
                return;
            }

            isFlipX = value;
            if ( uiCanvas != null )
            {
                // 우선 대충 처리..
                if ( isFlipX )
                {
                    transform.localScale = new Vector3( -1f, 1f, 1f );
                    uiCanvas.transform.localScale = new Vector3( -Mathf.Abs( uiCanvas.transform.localScale.x ), uiCanvas.transform.localScale.y, uiCanvas.transform.localScale.z );
                    EquipWeapon.transform.localScale = new Vector3( -1f, -1f, 1f );
                }
                else
                {
                    transform.localScale = Vector3.one;
                    uiCanvas.transform.localScale = new Vector3( Mathf.Abs( uiCanvas.transform.localScale.x ), uiCanvas.transform.localScale.y, uiCanvas.transform.localScale.z );
                    EquipWeapon.transform.localScale = Vector3.one;
                }
            }
        }
    }

    public Weapon EquipWeapon { get; private set; }
    public int UnattackableCount { get; set; }    // 0일때만 공격가능

    public Action<Character/*dead*/, Character/*attacker*/> OnDeadEvent;

    protected override void Awake()
    {
        base.Awake();
        EquipWeapon = GetComponentInChildren<Weapon>();
        Hp.Max = data.maxHp;
        Hp.SetMax();
    }

    public virtual void ApplyLookAngle( float _angle )
    {
        bool prevFlipX = IsFlipX;
        IsFlipX = ( _angle < -90f || _angle > 90f );
        EquipWeapon.LookAngle( prevFlipX != IsFlipX, _angle );
    }

    public virtual void OnHit( Character _attacker, Bullet _bullet )
    {
        if ( _attacker == null || _bullet == null )
        {
            Debug.LogWarning( $"Actor is null. attacker:{_attacker}, bullet:{_bullet}" );
        }

        Vector2 force = _bullet.data.pushingPower * _bullet.transform.up;
        Rigid2D.AddForce( force );

        Hp.Current -= _bullet.GetDamage();
        if ( Hp.Current <= 0 )
        {
            OnDeadEvent?.Invoke( this, _attacker  );
        }
    }
}

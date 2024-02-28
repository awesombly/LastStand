using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Character : Actor
{
    public CharacterData data;
    public bool IsDead { get; set; }
    public float LookAngle { get; set; }
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
                    if ( !ReferenceEquals( EquipWeapon, null ) )
                    {
                        EquipWeapon.transform.localScale = new Vector3( -1f, -1f, 1f );
                    }
                }
                else
                {
                    transform.localScale = Vector3.one;
                    uiCanvas.transform.localScale = new Vector3( Mathf.Abs( uiCanvas.transform.localScale.x ), uiCanvas.transform.localScale.y, uiCanvas.transform.localScale.z );
                    if ( !ReferenceEquals( EquipWeapon, null ) )
                    {
                        EquipWeapon.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }

    private Weapon equipWeapon;
    public Weapon EquipWeapon 
    {
        get => equipWeapon;
        protected set
        {
            if ( ReferenceEquals( equipWeapon, value ) )
            {
                return;
            }

            Weapon oldWeapon = equipWeapon;
            if ( !ReferenceEquals( oldWeapon, null ) )
            {
                oldWeapon.SetActiveWeapon( false );
            }

            equipWeapon = value;
            if ( !ReferenceEquals( equipWeapon, null ) )
            {
                equipWeapon.SetActiveWeapon( true );
            }

            OnChangeEquipWeapon?.Invoke( oldWeapon, equipWeapon );
        }
    }
    public int UnactionableCount { get; set; }    // 0일때만 행동가능

    public event Action<Weapon/*old*/, Weapon/*new*/> OnChangeEquipWeapon;

    protected override void Awake()
    {
        base.Awake();
        Hp.Max = data.maxHp;
        Hp.SetMax();
    }

    protected virtual void Start()
    {
    }

    public virtual void ApplyLookAngle( float _angle )
    {
        bool prevFlipX = IsFlipX;
        IsFlipX = ( _angle < -90f || _angle > 90f );
        LookAngle = _angle;
        EquipWeapon?.LookAngle( prevFlipX != IsFlipX, _angle );
    }
}

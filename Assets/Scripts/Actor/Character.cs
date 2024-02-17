using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Character : Actor
{
    public Global.StatusFloat Hp;
    public CharacterData data;
    [SerializeField]
    private GameObject playerCanvas;
    public Action<Character/*dead*/, Character/*attacker*/> OnDeadEvent;

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
            if ( playerCanvas != null )
            {
                // 快急 措面 贸府..
                if ( isFlipX )
                {
                    playerCanvas.transform.localScale = new Vector3( -Mathf.Abs( playerCanvas.transform.localScale.x ), playerCanvas.transform.localScale.y, playerCanvas.transform.localScale.z );
                }
                else
                {
                    playerCanvas.transform.localScale = new Vector3( Mathf.Abs( playerCanvas.transform.localScale.x ), playerCanvas.transform.localScale.y, playerCanvas.transform.localScale.z );
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Hp.Max = data.maxHp;
        Hp.SetMax();
    }

    public virtual void OnHit( Character _attacker, Bullet _bullet )
    {
        if ( _attacker == null || _bullet == null )
        {
            Debug.LogWarning( $"Actor is null. attacker:{_attacker}, bullet:{_bullet}" );
        }

        Vector2 force = _bullet.stat.pushingPower * _bullet.transform.up;
        Rigid2D.AddForce( force );

        Hp.Current -= ( _bullet.stat.damage * _attacker.data.attackRate );
        if ( Hp.Current <= 0 )
        {
            OnDeadEvent?.Invoke( this, _attacker  );
        }
    }
}

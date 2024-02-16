using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Character : Actor
{
    public Global.StatusFloat Hp;
    [SerializeField]
    protected CharacterData data;
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

    public Rigidbody2D Rigid2D { get; private set; }

    protected virtual void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();
        Hp.Max = data.maxHp;
        Hp.SetMax();
    }

    public virtual void OnHit( Character _attacker, float _damage, Vector2 _force )
    {
        if ( _force != Vector2.zero )
        {
            Rigid2D.AddForce( _force );
        }

        Hp.Current -= _damage;
        if ( Hp.Current <= 0 )
        {
            OnDeadEvent?.Invoke( this, _attacker );
        }
    }
}

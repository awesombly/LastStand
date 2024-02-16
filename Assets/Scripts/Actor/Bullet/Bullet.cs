using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Actor
{
    public Global.LayerValue targetLayer;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float damage;

    [HideInInspector]
    public uint ownerSerial;
    private Rigidbody2D rigid2D;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        if ( !gameObject.activeSelf )
        {
            return;
        }

        if ( !Global.CompareLayer( targetLayer, _other.gameObject.layer ) )
        {
            return;
        }

        Character attacker = GameManager.Inst.GetActor( ownerSerial ) as Character;
        Character otherCharacter = _other.GetComponent<Character>();
        otherCharacter?.HitAttack( attacker, damage );
        Release();
    }

    public void Fire()
    {
        rigid2D.velocity = transform.up * moveSpeed;
    }
}

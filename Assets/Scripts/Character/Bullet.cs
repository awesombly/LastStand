using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolObject
{
    [SerializeField]
    private LayerMask targetLayer;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float attackDamage;

    private Rigidbody2D rigid2D;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = transform.up * moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + delta );
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        if ( (1 << _other.gameObject.layer) != targetLayer )
        {
            return;
        }

        Enemy enemy = _other.GetComponent<Enemy>();
        enemy?.HitDamage( attackDamage );
        Release();
    }
}

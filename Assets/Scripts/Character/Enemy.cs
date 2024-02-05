using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : PoolObject
{
    [SerializeField]
    private EnemyData enemyData;
    private float curHp;

    private Rigidbody2D target;

    private Rigidbody2D rigid2D;
    private SpriteRenderer spriter;
    private Animator animator;

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        target = GameManager.Inst.player.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 movePosition = ( target.position - rigid2D.position ).normalized * enemyData.moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + movePosition );

        spriter.flipX = rigid2D.position.x > target.position.x;
    }

    private void OnTriggerExit2D( Collider2D _other )
    {
        if ( 1 << _other.gameObject.layer == enemyData.enemyArea )
        {
            Vector2 newPos = ( _other.gameObject.transform.position - transform.position ) * 2;
            transform.Translate( newPos );
        }
    }

    public void HitDamage( float _damage )
    {
        SetHp( curHp - _damage );
    }

    public void SetHp( float _hp )
    {
        curHp = Mathf.Min( _hp, enemyData.maxHp );
        if ( curHp <= 0 )
        {
            OnDead();
        }
    }

    private void OnDead()
    {
        /// +시체 처리, 점수 처리 등
        Release();
    }
}

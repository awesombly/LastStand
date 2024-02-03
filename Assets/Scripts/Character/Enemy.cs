using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : PoolObject
{
    [SerializeField]
    private EnemyData enemyData;

    private float curHp;
    public float CurHp
    {
        get { return curHp; }
        private set
        {
            curHp = Mathf.Min( value, enemyData.maxHp );
            if ( curHp <= 0 )
            {
                OnDead();
            }
        }
    }

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
        if ( _other.gameObject.layer.Equals( LayerMask.NameToLayer( "EnemyArea" ) ) )
        {
            Vector2 newPos = ( _other.gameObject.transform.position - transform.position ) * 2;
            transform.Translate( newPos );
        }
    }

    private void OnDead()
    {
        /// +시체 처리, 점수 처리 등
        Release();
    }
}

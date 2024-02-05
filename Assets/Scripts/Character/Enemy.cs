using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PoolObject
{
    public float Hp { get; set; }

    [SerializeField]
    private EnemyData enemyData;

    private Rigidbody2D rigid;
    private Rigidbody2D target;

    private void Start()
    {
        rigid  = GetComponent<Rigidbody2D>();
        target = GameManager.Inst.player.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var normalize = ( target.position - rigid.position ).normalized;

        Vector2 movePos = normalize * enemyData.moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition( rigid.position + movePos );
    }

    public void HitDamage( float _damage )
    {
        SetHp( Hp - _damage );
    }

    public void SetHp( float _hp )
    {
        Hp = Mathf.Min( _hp, enemyData.maxHp );
        if ( Hp <= 0 )
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

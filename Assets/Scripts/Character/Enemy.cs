using System;
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
    private SpriteRenderer rdr;

    public enum EnemyState { Idle = 0, Chase, Attack, Reload, }
    public EnemyState state;

    private Coroutine coroutine;

    private float attackRange;

    #region Unity Callback
    private void Awake()
    {
        rdr    = GetComponent<SpriteRenderer>();
        rigid  = GetComponent<Rigidbody2D>();
        target = GameManager.Inst.player.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ChangeState( EnemyState.Idle );
    }
    #endregion

    #region FSM
    private void ChangeState( EnemyState _state )
    {
        if ( !ReferenceEquals( coroutine, null ) )
             StopCoroutine( coroutine );

        state = _state;
        coroutine = StartCoroutine( state.ToString() );
    }

    private IEnumerator Idle()
    {
        yield return new WaitForSeconds( 1f );
        attackRange = UnityEngine.Random.Range( 3f, 12f );

        ChangeState( EnemyState.Chase );
    }

    private IEnumerator Chase()
    {
        while ( true )
        {
            yield return null;

            rdr.flipX = ( target.position.x - rigid.position.x ) < 0;

            var distance = Vector2.Distance( target.position, rigid.position );
            if ( distance <= attackRange )
            {
                ChangeState( EnemyState.Attack );
                yield break;
            }

            Vector2 dir = ( target.position - rigid.position ).normalized;
            rigid.position += dir * enemyData.moveSpeed * Time.deltaTime;
        }
    }

    private IEnumerator Attack()
    {
        while ( true )
        {
            yield return new WaitForSeconds( 1f );
            ChangeState( EnemyState.Idle );
        }
    }
    #endregion

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

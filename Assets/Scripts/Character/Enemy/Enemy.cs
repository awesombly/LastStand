using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "Enemy", menuName = "Scriptable Objects/Default" )]
public class EnemyData : ScriptableObject
{
    public float maxHp;
    public float speed;
    public float damage;
}

public class Enemy : PoolObject
{
    public float Hp { get; set; }

    [SerializeField]
    private EnemyData data;

    private Rigidbody2D rigid;
    private Rigidbody2D target;
    private SpriteRenderer rdr;
    public float attackRange;

    private enum EnemyState { Idle = 0, Chase, Attack, Reload, }
    private Coroutine coroutine;

    #region Unity Callback
    private void Awake()
    {
        rdr    = GetComponent<SpriteRenderer>();
        rigid  = GetComponent<Rigidbody2D>();
        target = GameManager.Inst.player.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        attackRange = UnityEngine.Random.Range( 3f, 10f );
        ChangeState( EnemyState.Idle );
    }
    #endregion

    #region FSM
    private void ChangeState( EnemyState _state )
    {
        if ( !ReferenceEquals( coroutine, null ) )
             StopCoroutine( coroutine );

        coroutine = StartCoroutine( _state.ToString() );
    }

    private IEnumerator Idle()
    {
        yield return YieldCache.WaitForSeconds( 1f );

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
            rigid.position += dir * data.speed * Time.deltaTime;
        }
    }

    private IEnumerator Attack()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( 1f );
            ChangeState( EnemyState.Idle );
        }
    }
    #endregion

    public void Initialize( Vector3 _pos )
    {
        transform.position = _pos;

        ChangeState( EnemyState.Idle );
    }

    public void HitDamage( float _damage )
    {
        Hp -= _damage;
        if ( Hp < 0 )
        {
            if ( !ReferenceEquals( coroutine, null ) )
                 StopCoroutine( coroutine );
             
            Release();
        }
    }
}

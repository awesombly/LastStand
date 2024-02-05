using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public Vector2 Direction { get; private set; }

    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private LayerMask targetLayer;
    [SerializeField]
    private float moveSpeed;

    private Vector2 inputVector;
    private Vector2 prevPosition;

    private Rigidbody2D rigid2D;
    private SpriteRenderer spriter;
    private Animator animator;

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = inputVector * moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + delta );

        Direction = ( rigid2D.position - prevPosition ).normalized;
        prevPosition = rigid2D.position;
    }

    private void LateUpdate()
    {
        animator.SetFloat( "MoveSpeed", inputVector.sqrMagnitude );

        if ( inputVector.x != 0f )
        {
            spriter.flipX = inputVector.x < 0f;
        }
    }

    private void OnMove( InputValue _value )
    {
        inputVector = _value.Get<Vector2>();
    }

    private void OnAttack()
    {
        Collider2D enemy = GetNearestTarget( 30 );
        if ( enemy == null )
        {
            Debug.Log( "null" );
            return;
        }

        PoolObject bulletObj = PoolManager.Inst.Get( bullet );
        bulletObj.transform.position = transform.position;

        Vector3 dir = ( enemy.transform.position - transform.position ).normalized;
        bulletObj.transform.rotation = Quaternion.FromToRotation( Vector3.up, dir );
    }

    private Collider2D GetNearestTarget( float _radius )
    {
        float minDistance = float.MaxValue;
        Collider2D target = null;

        Collider2D[] enemies = Physics2D.OverlapCircleAll( transform.position, _radius, targetLayer );
        for ( int i = 0; i < enemies.Length; ++i )
        {
            float distance = Vector2.Distance( transform.position, enemies[i].transform.position );
            if ( distance < minDistance )
            {
                minDistance = distance;
                target = enemies[i];
            }
        }

        return target;
    }
}

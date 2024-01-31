using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D target;
    [SerializeField]
    private float moveSpeed;

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
        Vector2 movePosition = ( target.position - rigid2D.position ).normalized * moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + movePosition );

        spriter.flipX = rigid2D.position.x > target.position.x;
    }
}

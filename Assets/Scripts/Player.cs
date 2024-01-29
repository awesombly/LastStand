using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float moveSpeed;
    
    private Rigidbody2D rigid2D;

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        inputVec.x = Input.GetAxisRaw( "Horizontal" );
        inputVec.y = Input.GetAxisRaw( "Vertical" );
    }

    private void FixedUpdate()
    {
        Vector2 moveVector = inputVec.normalized * moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + moveVector );
    }
}

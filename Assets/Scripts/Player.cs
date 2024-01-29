using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public Vector2 inputVector;
    public float moveSpeed;
    
    private Rigidbody2D rigid2D;

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }
   
    private void FixedUpdate()
    {
        Vector2 moveVector = inputVector * moveSpeed * Time.fixedDeltaTime;
        rigid2D.MovePosition( rigid2D.position + moveVector );
    }

    private void OnMove( InputValue _value )
    {
        inputVector = _value.Get<Vector2>();
    }
}

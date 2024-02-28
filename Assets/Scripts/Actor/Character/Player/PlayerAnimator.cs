using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject handLeft;
    [SerializeField]
    private GameObject handRight;
    private Vector3 handLeftPosition;
    private Vector3 handRightPosition;

    private Animator animator;
    private PlayerMovement movement;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        handLeftPosition = handLeft.transform.localPosition;
        handRightPosition = handRight.transform.localPosition;
    }

    private void Update()
    {
        // Hand Shaking
        Vector3 delta = Vector3.up * ( Mathf.Sin( Time.time * 20f ) * 0.05f );
        handLeft.transform.localPosition = handLeftPosition + delta;
        handRight.transform.localPosition = handRightPosition + delta;
    }

    private void LateUpdate()
    {
        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat( AnimatorParameters.MoveSpeed, movement.moveInfo.moveVector.sqrMagnitude );
        animator.SetInteger( AnimatorParameters.LookDirection, ( int )GetLookDirection() );
    }

    private LookDirection GetLookDirection()
    {
        switch ( GameManager.LookAngle )
        {
            case > 157.5f:
                return LookDirection.Right;
            case > 112.5f:
                return LookDirection.BackRight;
            case > 67.5f:
                return LookDirection.Back;
            case > 22.5f:
                return LookDirection.BackRight;
            case > -45f:
                return LookDirection.Right;
            case > -135f:
                return LookDirection.Front;
            default:
                return LookDirection.Right;
        }
    }

    private LookDirection GetLookDirection8Way()
    {
        switch ( GameManager.LookAngle )
        {
            case > 157.5f:
                return LookDirection.Left;
            case > 112.5f:
                return LookDirection.BackLeft;
            case > 67.5f:
                return LookDirection.Back;
            case > 22.5f:
                return LookDirection.BackRight;
            case > -22.5f:
                return LookDirection.Right;
            case > -67.5f:
                return LookDirection.FrontRight;
            case > -112.5f:
                return LookDirection.Front;
            case > -157.5f:
                return LookDirection.FrontLeft;
            default:
                return LookDirection.Left;
        }
    }

    private enum LookDirection
    {
        Left = 0, BackLeft, Back, BackRight, Right, FrontRight, Front, FrontLeft,
    }

    private static class AnimatorParameters
    {
        public static int MoveSpeed = Animator.StringToHash( "MoveSpeed" );
        public static int LookDirection = Animator.StringToHash( "LookDirection" );
    }
}

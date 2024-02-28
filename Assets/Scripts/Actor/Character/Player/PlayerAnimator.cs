using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        //animator.SetFloat( "MoveSpeed", moveInfo.moveVector.sqrMagnitude );
        animator.SetInteger( AnimatorParameters.LookDirection, ( int )GetLookDirection() );
    }

    private LookDirection GetLookDirection()
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
        public static int LookDirection = Animator.StringToHash( "LookDirection" );
    }
}

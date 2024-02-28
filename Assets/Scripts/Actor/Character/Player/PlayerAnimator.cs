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

    private Player player;
    private PlayerMovement movement;
    private Animator animator;

    private void Awake()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        handLeftPosition = handLeft.transform.localPosition;
        handRightPosition = handRight.transform.localPosition;

        movement.OnDodgeAction += OnDodgeAction;
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
        animator.SetInteger( AnimatorParameters.LookDirection, ( int )GetAnimatorDirection( GameManager.LookAngle ) );
        animator.SetBool( AnimatorParameters.IsActionBlocked, player.UnactionableCount > 0 );
    }

    private void OnDodgeAction( bool _isActive )
    {
        float actionAngle = Global.GetAngle( Vector3.zero, movement.GetDodgeDirection() );
        player.ApplyLookAngle( actionAngle );

        animator.SetInteger( AnimatorParameters.ActionDirection, ( int )GetAnimatorDirection( actionAngle ) );
        animator.SetBool( AnimatorParameters.DodgeAction, _isActive );
    }

    private LookDirection GetAnimatorDirection( float angle )
    {
        switch ( angle )
        {
            case > 157.5f:
                return LookDirection.Right;
            case > 112.5f:
                return LookDirection.BackRight;
            case > 67.5f:
                return LookDirection.Back;
            case > 22.5f:
                return LookDirection.BackRight;
            case >= -45f:
                return LookDirection.Right;
            case > -135f:
                return LookDirection.Front;
            default:
                return LookDirection.Right;
        }
    }

    private LookDirection GetAnimatorDirection8Way( float angle )
    {
        switch ( angle )
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
        public static int ActionDirection = Animator.StringToHash( "ActionDirection" );

        public static int IsActionBlocked = Animator.StringToHash( "IsActionBlocked" );
        public static int DodgeAction = Animator.StringToHash( "DodgeAction" );
    }
}

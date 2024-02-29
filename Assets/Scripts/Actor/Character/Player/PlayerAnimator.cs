using DG.Tweening;
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
    private float elapsedTimeForHand;

    private Sequence fireSequence;

    private Player player;
    private PlayerMovement movement;
    private Animator animator;

    #region Unity Callback
    private void Awake()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        player.OnChangeEquipWeapon += OnChangeEquipWeapon;
        movement.OnDodgeAction += OnDodgeAction;

        handLeftPosition = handLeft.transform.localPosition;
        handRightPosition = handRight.transform.localPosition;
    }

    private void Update()
    {
        // Hand Shaking
        player.IsOnFire = !ReferenceEquals( fireSequence, null ) && ( fireSequence.IsActive() || fireSequence.IsPlaying() );
        if ( !player.IsOnFire )
        {
            elapsedTimeForHand += Time.deltaTime;
            Vector3 delta = Vector3.up * ( Mathf.Sin( elapsedTimeForHand * 20f ) * 0.05f );
            //handLeft.transform.localPosition = handLeftPosition + delta;
            handRight.transform.localPosition = handRightPosition + delta;
        }
    }

    private void LateUpdate()
    {
        UpdateAnimatorParameters();
    }
    #endregion

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat( AnimatorParameters.MoveSpeed, movement.moveInfo.moveVector.sqrMagnitude );
        animator.SetInteger( AnimatorParameters.LookDirection, ( int )GetAnimatorDirection( player.LookAngle ) );
        animator.SetBool( AnimatorParameters.IsActionBlocked, player.UnactionableCount > 0 );
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction )
    {
        float actionAngle = Global.GetAngle( Vector3.zero, _direction );
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

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old != null )
        {
            _old.OnFireEvent -= OnFire;
        }

        if ( _new != null )
        {
            _new.OnFireEvent += OnFire;
        }
    }

    private void OnFire( Weapon _weapon )
    {
        // 적당히 계산한 값
        float positionStrength = 0.17f + ( _weapon.data.stat.reactionPower / 3000f );
        float rotationStrength = 22f + ( _weapon.data.stat.reactionPower / 60f );

        fireSequence?.Kill();
        fireSequence = DOTween.Sequence().SetAutoKill( true )
            .Append( handRight.transform.DOShakePosition( _weapon.data.stat.repeatDelay * 0.6f, positionStrength ) )
            .Join( handRight.transform.DOShakeRotation( _weapon.data.stat.repeatDelay * 0.6f, Vector3.forward * rotationStrength ) )
            .OnKill( () => fireSequence = null );
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

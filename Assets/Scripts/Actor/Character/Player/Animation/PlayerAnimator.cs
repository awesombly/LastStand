using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Poolable deadPrefab;

    [SerializeField]
    private SpriteRenderer handLeft;
    [SerializeField]
    private SpriteRenderer handRight;
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

        player.OnChangePlayerType += OnChangePlayerType;
        player.OnChangeEquipWeapon += OnChangeEquipWeapon;
        player.OnInteractionAction += OnInteractionAction;
        movement.OnDodgeAction += OnDodgeAction;

        handLeftPosition = handLeft.transform.localPosition;
        handRightPosition = handRight.transform.localPosition;
    }

    private void Update()
    {
        // Hand Shaking
        player.IsOnFire = fireSequence is not null && ( fireSequence.IsActive() || fireSequence.IsPlaying() );
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

    private void OnDestroy()
    {
        fireSequence?.Kill();
    }
    #endregion

    public void SetActionSpeed( float _length, float _duration )
    {
        animator.SetFloat( AnimatorParameters.ActionSpeed, _length / _duration );
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat( AnimatorParameters.MoveSpeed, movement.moveInfo.moveVector.sqrMagnitude );
        animator.SetInteger( AnimatorParameters.LookDirection, ( int )GetAnimatorDirection( player.LookAngle ) );
        animator.SetBool( AnimatorParameters.IsActionBlocked, player.UnactionableCount > 0 );
    }

    private void OnDodgeAction( bool _isActive, Vector2 _direction, float _duration )
    {
        float actionAngle = Global.GetAngle( Vector2.zero, _direction );
        player.ApplyLookAngle( actionAngle );
        handRight.gameObject.SetActive( !_isActive );

        SetActionSpeed( 0.75f, _duration );
        animator.SetInteger( AnimatorParameters.ActionDirection, ( int )GetAnimatorDirection( actionAngle ) );
        animator.SetBool( AnimatorParameters.DodgeAction, _isActive );
    }

    public void SetReviveAction( bool _isActive )
    {
        animator.SetTrigger( AnimatorParameters.ReviveAction );
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

    private void OnChangePlayerType( PlayerType _type )
    {
        PlayerSO data = player.playerData;

        animator.runtimeAnimatorController = data.playerAC;
        handLeft.sprite = data.handSprite;
        handRight.sprite = data.handSprite;
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old is not null )
        {
            _old.OnFireEvent -= OnFire;
        }

        if ( _new is not null )
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

    private void OnInteractionAction( float _angle )
    {
        player.ApplyLookAngle( _angle );
        // 실질적인 ActionSpeed는 PlayerActionControlSM에서 설정하고 있다
        // ActionSpeed 적용 전의 Animation length를 얻기 위해 초기화 해준다
        animator.SetFloat( AnimatorParameters.ActionSpeed, 1f );
        animator.SetInteger( AnimatorParameters.ActionDirection, ( int )GetAnimatorDirection( _angle ) );
        animator.SetTrigger( AnimatorParameters.KickAction );
    }

    public void OnDead( Vector2 _direction )
    {
        Poolable poolable = PoolManager.Inst.Get( deadPrefab );
        poolable.transform.position = transform.position;

        Animator deadAnimator = poolable.GetComponent<Animator>();
        deadAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
        deadAnimator.SetBool( AnimatorParameters.IsActionBlocked, true );
        deadAnimator.SetTrigger( AnimatorParameters.DeathAction );

        Rigidbody2D rigid = poolable.GetComponent<Rigidbody2D>();
        rigid.AddForce( _direction * 700f );

        poolable.StartCoroutine( RemoveDead( poolable, 5f ) );
    }

    private IEnumerator RemoveDead( Poolable _dead, float _delay )
    {
        yield return YieldCache.WaitForSeconds( _delay );
        _dead.Release();
    }

    private enum LookDirection
    {
        Left = 0, BackLeft, Back, BackRight, Right, FrontRight, Front, FrontLeft,
    }

}

public static partial class AnimatorParameters
{
    public static int MoveSpeed = Animator.StringToHash( "MoveSpeed" );
    public static int LookDirection = Animator.StringToHash( "LookDirection" );
    public static int ActionDirection = Animator.StringToHash( "ActionDirection" );

    public static int IsActionBlocked = Animator.StringToHash( "IsActionBlocked" );
    public static int ActionSpeed = Animator.StringToHash( "ActionSpeed" );
    public static int DodgeAction = Animator.StringToHash( "DodgeAction" );
    public static int ReviveAction = Animator.StringToHash( "ReviveAction" );
    public static int DeathAction = Animator.StringToHash( "DeathAction" );
    public static int DanceAction = Animator.StringToHash( "DanceAction" );
    public static int DefeatAction = Animator.StringToHash( "DefeatAction" );
    public static int KickAction = Animator.StringToHash( "KickAction" );
}

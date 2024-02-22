using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerMovement : MonoBehaviour
{
    [Serializable]
    public struct AllowSyncInfo
    {
        public float distance;
        public float angle;
        public Global.StatusFloat interval;
    }
    [SerializeField]
    private AllowSyncInfo syncInfo;

    public struct MovementInfo
    {
        public Vector2 moveVector;
        public Vector3 prevMoveVector;
        public Vector2 prevPosition;
        public bool prevIsSleep;
        public float prevAngle;
    }
    public MovementInfo moveInfo;

    [Serializable]
    public struct DodgeInfo
    {
        public bool useCollision;
        public float moveSpeed;
        public float range;
        public Global.StatusFloat cooldown;
        [HideInInspector]
        public Global.StatusFloat duration;
        [HideInInspector]
        public Vector2 direction;
    }
    [SerializeField]
    private DodgeInfo dodgeInfo;

    #region Components
    private Player player;
    private ActionReceiver receiver;
    private Rigidbody2D rigid2D;
    private Animator animator;
    private CapsuleCollider2D capsuleCollider;
    #endregion

    #region Unity Callback
    private void Awake()
    {
        player = GetComponent<Player>();
        receiver = GetComponent<ActionReceiver>();
        rigid2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        receiver.OnDodgeEvent += OnDodge;
        dodgeInfo.duration.OnChangeCurrent += OnChangeDodgeDuration;
    }

    private void OnEnable()
    {
        moveInfo.moveVector = Vector2.zero;
    }

    private void Update()
    {
        if ( !player.IsLocal )
        {
            return;
        }

        syncInfo.interval.Current -= Time.deltaTime;
        UpdateLookAngle();
    }

    private void FixedUpdate()
    {
        dodgeInfo.duration.Current -= Time.deltaTime;
        if ( dodgeInfo.duration.Current > 0f )
        {
            return;
        }
        dodgeInfo.cooldown.Current -= Time.deltaTime;

        if ( player.IsLocal )
        {
            moveInfo.moveVector = receiver.InputVector * player.data.moveSpeed;
            ReqSyncMovement();
        }

        rigid2D.MovePosition( rigid2D.position + moveInfo.moveVector * Time.fixedDeltaTime );

        player.Direction = ( rigid2D.position - moveInfo.prevPosition ).normalized;
        moveInfo.prevPosition = rigid2D.position;
        moveInfo.prevMoveVector = moveInfo.moveVector;
        moveInfo.prevIsSleep = rigid2D.IsSleeping();
    }

    private void LateUpdate()
    {
        animator.SetFloat( "MoveSpeed", moveInfo.moveVector.sqrMagnitude );
    }
#endregion

    private void ReqSyncMovement()
    {
        bool isStopped = !moveInfo.prevIsSleep && rigid2D.IsSleeping();
        float velocityInterval = Vector2.Distance( moveInfo.moveVector, moveInfo.prevMoveVector );
        if ( isStopped || velocityInterval >= syncInfo.distance )
        {
            MOVEMENT_INFO protocol;
            protocol.serial = player.Serial;
            protocol.pos = new VECTOR2( rigid2D.position );
            protocol.vel = new VECTOR2( moveInfo.moveVector );
            Network.Inst.Send( PacketType.SYNC_MOVEMENT_REQ, protocol );
        }
    }

    private void UpdateLookAngle()
    {
        bool prevFlipX = player.IsFlipX;
        player.ApplyLookAngle( GameManager.LookAngle );

        #region ReqProtocol
        if ( syncInfo.interval.IsZero && player.Serial != uint.MaxValue
            && ( player.IsFlipX != prevFlipX || Mathf.Abs( GameManager.LookAngle - moveInfo.prevAngle ) >= syncInfo.angle ) )
        {
            syncInfo.interval.SetMax();

            LOOK_INFO protocol;
            protocol.serial = player.Serial;
            protocol.angle = GameManager.LookAngle;
            Network.Inst.Send( PacketType.SYNC_LOOK_ANGLE_REQ, protocol );
            moveInfo.prevAngle = GameManager.LookAngle;
        }
        #endregion
    }

    public void DodgeAction( bool _useCollision, Vector2 _direction, float _duration )
    {
        ++player.UnattackableCount;
        dodgeInfo.cooldown.SetMax();

        dodgeInfo.duration.Max = _duration;
        dodgeInfo.duration.SetMax();

        dodgeInfo.direction = _direction;

        capsuleCollider.isTrigger = !_useCollision;
    }

    private void OnDodge()
    {
        if ( !dodgeInfo.cooldown.IsZero )
        {
            return;
        }

        bool isAFK = ( receiver.InputVector.sqrMagnitude == 0f );
        Vector2 direction = isAFK ? GameManager.MouseDirection : receiver.InputVector;
        float duration = dodgeInfo.range / dodgeInfo.moveSpeed;

        DodgeAction( dodgeInfo.useCollision, direction, duration );

        DODGE_INFO protocol;
        protocol.serial = player.Serial;
        protocol.useCollision = dodgeInfo.useCollision;
        protocol.pos = new VECTOR2( rigid2D.position );
        protocol.dir = new VECTOR2( direction );
        protocol.dur = duration;
        Network.Inst.Send( PacketType.SYNC_DODGE_ACTION_REQ, protocol );
    }

    private void OnChangeDodgeDuration( float _old, float _new )
    {
        float deltaTime = ( _old - _new );
        if ( deltaTime <= 0f )
        {
            return;
        }

        Vector2 delta = dodgeInfo.direction * dodgeInfo.moveSpeed * deltaTime;
        rigid2D.MovePosition( rigid2D.position + delta );

        if ( dodgeInfo.duration.IsZero )
        {
            --player.UnattackableCount;
            capsuleCollider.isTrigger = false;
        }
    }
}

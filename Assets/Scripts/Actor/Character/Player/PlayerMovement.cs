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

    private Player player;
    private ActionReceiver receiver;
    private Rigidbody2D rigid2D;
    private Animator animator;

    #region Unity Callback
    private void Awake()
    {
        player = GetComponent<Player>();
        receiver = GetComponent<ActionReceiver>();
        rigid2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        if ( player.IsLocal )
        {
            moveInfo.moveVector = receiver.InputVector * player.data.moveSpeed;
            ReqSynkMovement();
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

    private void ReqSynkMovement()
    {
        bool isStopped = !moveInfo.prevIsSleep && rigid2D.IsSleeping();
        float velocityInterval = Vector2.Distance( moveInfo.moveVector, moveInfo.prevMoveVector );
        if ( isStopped || velocityInterval >= syncInfo.distance )
        {
            MOVEMENT_INFO protocol;
            protocol.serial = player.Serial;
            protocol.pos = new VECTOR2( rigid2D.position );
            protocol.vel = new VECTOR2( moveInfo.moveVector );
            Network.Inst.Send( PacketType.SYNK_MOVEMENT_REQ, protocol );
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
            Network.Inst.Send( PacketType.SYNK_LOOK_ANGLE_REQ, protocol );
            moveInfo.prevAngle = GameManager.LookAngle;
        }
        #endregion
    }
}

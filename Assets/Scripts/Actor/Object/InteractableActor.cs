using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableActor : DestroyableActor
{
    [SerializeField]
    private UnityEvent<float, bool> interactionEvent;
    [SerializeField]
    private UnityEvent targetAngleEvent;

    private float targetAngle;
    private bool isInteracted = false;
    private PolygonCollider2D polygon2D;

    private enum ActionDirection
    {
        Up = 0, Right, Down, Left
    }

    protected override void Awake()
    {
        base.Awake();
        polygon2D = GetComponent<PolygonCollider2D>();
    }

    public void InteractionAction( float _angle, bool _isInit = false )
    {
        // InGame 입장때 Interaction 여부 구별용도
        if ( _angle == 0f )
        {
            return;
        }
        interactionEvent?.Invoke( _angle, _isInit );
    }

    public void TryInteraction( Player _player )
    {
        if ( isInteracted || !_player.IsLocal )
        {
            return;
        }

        targetAngleEvent?.Invoke();
        InteractionAction( targetAngle );

        // Req Protocol
        LOOK_INFO protocol;
        protocol.serial = Serial;
        protocol.angle = targetAngle;
        Network.Inst.Send( PacketType.SYNC_INTERACTION_REQ, protocol );
    }

    public void UpdatePlayerAngle()
    {
        targetAngle = Global.GetAngle( GameManager.LocalPlayer.transform.position, transform.position );
    }

    public void UpdateMouseAngle()
    {
        targetAngle = GameManager.LookAngle;
    }

    private ActionDirection GetActionDirection( float angle )
    {
        switch ( angle )
        {
            case > 135f:
                return ActionDirection.Left;
            case > 45f:
                return ActionDirection.Up;
            case > -45f:
                return ActionDirection.Right;
            case > -135f:
                return ActionDirection.Down;
            default:
                return ActionDirection.Left;
        }
    }

    #region Interactions
    public void InteractionKickObject( float _angle, bool _isInit = false )
    {
        if ( _isInit )
        {
            return;
        }
        float radian = _angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2( Mathf.Cos( radian ), Mathf.Sin( radian ) );
        Rigid2D.AddForce( direction * 20000f );
    }

    public void InteractionFlipTable( float _angle, bool _isInit = false )
    {
        // 테이블 뒤집기
        isInteracted = true;
        ActionDirection direction = GetActionDirection( _angle );
        if ( direction == ActionDirection.Left )
        {
            transform.localScale = new Vector3( -1f, 1f, 1f );
        }
        Rigid2D.excludeLayers &= ~( int )Global.LayerFlag.PlayerAttack;
        animator.SetInteger( AnimatorParameters.ActionDirection, ( int )direction );
        animator.SetTrigger( AnimatorParameters.Interaction );

        StartCoroutine( UpdatePhysicsShape( .5f ) );
    }

    private IEnumerator UpdatePhysicsShape( float _duration )
    {
        if ( ReferenceEquals( polygon2D, null ) )
        {
            yield break;
        }

        // sprite가 바껴도 collider 업데이트가 안되서 수동으로 갱신
        while ( _duration > 0f )
        {
            _duration -= Time.deltaTime;
            yield return YieldCache.WaitForEndOfFrame;

            List<Vector2> lists = new List<Vector2>();
            spriter.sprite.GetPhysicsShape( 0, lists );
            polygon2D.SetPath( 0, lists );
        }
    }
    #endregion
}

public static partial class AnimatorParameters
{
    public static int Interaction = Animator.StringToHash( "Interaction" );
}

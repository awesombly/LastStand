using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableActor : DestroyableActor
{
    private bool isInteracted = false;
    private PolygonCollider2D collide2D;
    private SpriteRenderer spriter;

    protected override void Awake()
    {
        base.Awake();
        collide2D = GetComponent<PolygonCollider2D>();
        spriter = GetComponent<SpriteRenderer>();
    }

    public void Interaction( Player _player )
    {
        if ( isInteracted )
        {
            return;
        }
        isInteracted = true;
        float angle =  Global.GetAngle( _player.transform.position, transform.position );
        ActionDirection direction = GetActionDirection( angle );

        // 테이블 뒤집기
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
        // sprite가 바껴도 collider 업데이트가 안되서 수동으로 갱신
        while ( _duration > 0f )
        {
            _duration -= Time.deltaTime;
            yield return YieldCache.WaitForEndOfFrame;

            List<Vector2> lists = new List<Vector2>();
            spriter.sprite.GetPhysicsShape( 0, lists );
            collide2D.SetPath( 0, lists );
        }
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

    private enum ActionDirection
    {
        Up = 0, Right, Down, Left
    }
}

public static partial class AnimatorParameters
{
    public static int Interaction = Animator.StringToHash( "Interaction" );
}

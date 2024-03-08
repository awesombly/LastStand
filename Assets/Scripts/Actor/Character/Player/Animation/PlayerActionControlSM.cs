using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerActionControlSM : StateMachineBehaviour
{
    PlayerAnimator playerAnimator = null;
    public float actionDuration;

    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( playerAnimator is null )
        {
            playerAnimator = animator.GetComponent<PlayerAnimator>();
        }
        playerAnimator.SetActionSpeed( stateInfo.length, actionDuration );
    }
}

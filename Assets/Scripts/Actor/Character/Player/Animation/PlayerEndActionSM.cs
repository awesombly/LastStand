using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEndActionSM : StateMachineBehaviour
{
    private Player player = null;

    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( player is null )
        {
            player = animator.GetComponent<Player>();
        }
        --player.UnactionableCount;
    }
}

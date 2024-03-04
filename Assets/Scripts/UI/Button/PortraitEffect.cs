using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( SpriteRenderer ) )]
public class PortraitEffect : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer rdr;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rdr  = GetComponent<SpriteRenderer>();
        rdr.color = Color.gray;
    }

    public void OnPointerEnter()
    {
        AudioManager.Inst.Play( SFX.MenuHover );
        anim.SetBool( "IsHover", true );
        rdr.DOColor( Color.white, .35f );
        transform.DOScale( 1.25f, .35f );
    }

    public void OnPointerExit()
    {
        anim.SetBool( "IsHover", false );
        rdr.DOColor( Color.gray, .35f );
        transform.DOScale( 1f, .35f );
    }
}
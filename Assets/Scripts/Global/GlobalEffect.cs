using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( SpriteRenderer ) )]
public class GlobalEffect : Singleton<GlobalEffect>
{
    private SpriteRenderer sprite;
    private static readonly float FadeTime = .5f;

    protected override void Awake()
    {
        base.Awake();

        if ( TryGetComponent( out sprite ) )
        {
            Texture2D tex        = Texture2D.whiteTexture;
            gameObject.layer     = Global.Layer.UI;
            sprite.sprite        = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
            sprite.drawMode      = SpriteDrawMode.Sliced;
            sprite.size          = new Vector2( 10000f, 10000f );
            sprite.sortingOrder  = 1000;
            transform.position   = new Vector3( 0f, 0f, 10f );
            transform.localScale = Vector3.one;
        }
    }

    public void FadeIn( Action _onCompleted = null ) => StartCoroutine( SceneFadeIn( _onCompleted ) );

    public void FadeOut( Action _onCompleted = null ) => StartCoroutine( SceneFadeOut( _onCompleted ) );

    private IEnumerator SceneFadeIn( Action _onCompleted = null )
    {
        SceneBase.EnabledInputSystem( false, false );
        sprite.color = Color.black;
        sprite.enabled = true;

        float alpha = 1f;
        while ( alpha >= 0f )
        {
            yield return null;
            sprite.color = new Color( 0f, 0f, 0f, alpha -= Time.deltaTime / FadeTime );
        }

        sprite.color = Color.clear;
        SceneBase.EnabledInputSystem( true, true );
        sprite.enabled = false;
        _onCompleted?.Invoke();
    }

    private IEnumerator SceneFadeOut( Action _onCompleted = null )
    {
        SceneBase.EnabledInputSystem( false, false );
        sprite.color = Color.clear;
        sprite.enabled = true;

        float alpha = 0f;
        while ( alpha <= 1f )
        {
            yield return null;
            sprite.color = new Color( 0f, 0f, 0f, alpha += Time.deltaTime / FadeTime );
        }

        sprite.color = Color.black;
        _onCompleted?.Invoke();
    }
}

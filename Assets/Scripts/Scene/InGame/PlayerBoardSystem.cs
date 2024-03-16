using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerBoardSystem : MonoBehaviour
{
    public GameObject contents;
    public  RectTransform hintRT;
    private RectTransform boardRT;
    [SerializeField] List<PlayerBoard> boards = new List<PlayerBoard>();
    [SerializeField] List<Vector2>     points = new List<Vector2>();
    private bool isMovePlaying;
    
    private void Awake()
    {
        boards.AddRange( contents.GetComponentsInChildren<PlayerBoard>( true ) );
        foreach ( var board in boards )
            points.Add( ( board.transform as RectTransform ).anchoredPosition );

        boardRT = contents.transform as RectTransform;

        GameManager.OnChangePlayers += OnChangePlayers;
        GameManager.OnDead          += UpdateOrderByKill;
    }

    private void OnDestroy()
    {
        for ( int i = 0; i < 4; i++ )
              boards[i].RemoveEvents();

        DOTween.Kill( hintRT );
        DOTween.Kill( boardRT );

        GameManager.OnChangePlayers -= OnChangePlayers;
        GameManager.OnDead          -= UpdateOrderByKill;
    }

    private void Update()
    {
        if ( SceneBase.IsLock )
             return;

        if ( Input.GetKeyDown( KeyCode.Tab ) )
        {
            if ( isMovePlaying )
                 return;

            if ( contents.activeInHierarchy )
            {
                isMovePlaying = true;
                AudioManager.Inst.Play( SFX.MenuExit );
                hintRT.DOAnchorPosX( -75f, .5f );
                boardRT.DOAnchorPosX( -160f, .5f )
                       .OnComplete( () =>
                       {
                           contents.SetActive( false );
                           isMovePlaying     = false;
                       } );
            }
            else
            {
                AudioManager.Inst.Play( SFX.MenuEntry );
                contents.SetActive( true );
                isMovePlaying = true;
                boardRT.DOAnchorPosX( 160f, .5f ).OnComplete( () => isMovePlaying = false );
                hintRT.DOAnchorPosX( 130f, .5f );
            }
        }
    }

    private void OnChangePlayers()
    {
        var players = GameManager.Players;
        for ( int i = 0; i < 4; i++ )
        {
            boards[i].RemoveEvents();

            if ( i < players.Count )
            {
                boards[i].gameObject.SetActive( true );
                boards[i].AddEvents( players[i] );
            }
            else boards[i].gameObject.SetActive( false );
        }

        UpdateOrderByKill( null );
    }

    private void UpdateOrderByKill( Player _p )
    {
        var players = GameManager.Players;
        for ( int i = 0; i < players.Count; i++ )
        {
            var board = players[i].Board;
            board.MoveToPosition( points[i] );
            board.transform.SetAsFirstSibling();
        }
    }
}
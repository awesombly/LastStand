using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerBoardSystem : MonoBehaviour
{
    public GameObject contents;
    public RectTransform hintRT;
    [SerializeField] List<PlayerBoard> boards = new List<PlayerBoard>();
    [SerializeField] List<Vector2>     points = new List<Vector2>();
    private bool isMovePlaying;
    
    private void Awake()
    {
        boards.AddRange( contents.GetComponentsInChildren<PlayerBoard>( true ) );
        foreach ( var board in boards )
            points.Add( ( board.transform as RectTransform ).anchoredPosition );

        GameManager.OnChangePlayers += OnChangePlayers;
        GameManager.OnDead          += UpdateOrderByKill;
    }

    private void OnDestroy()
    {
        for ( int i = 0; i < 4; i++ )
              boards[i].RemoveEvents();

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

            RectTransform boardRT = contents.transform as RectTransform;
            if ( contents.activeInHierarchy )
            {
                AudioManager.Inst.Play( SFX.MenuExit );
                isMovePlaying = true;
                boardRT.DOAnchorPosX( -1125f, .5f )
                       .OnComplete( () =>
                       {
                           contents.SetActive( false );
                           isMovePlaying = false;
                       } );

                hintRT.DOAnchorPosX( -1050f, .5f );
            }
            else
            {
                AudioManager.Inst.Play( SFX.MenuEntry );
                contents.SetActive( true );
                isMovePlaying = true;
                boardRT.DOAnchorPosX( -800f, .5f ).OnComplete( () => { isMovePlaying = false; } );
                hintRT.DOAnchorPosX( -830f, .5f );
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
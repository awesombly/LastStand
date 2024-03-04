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
        boards.AddRange( contents.GetComponentsInChildren<PlayerBoard>() );
        foreach ( var board in boards )
            points.Add( ( board.transform as RectTransform ).anchoredPosition );

        GameManager.OnChangePlayers += UpdatePlayerBoard;
        GameManager.OnDead          += UpdateOrderByKill;
    }

    private void Update()
    {
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

    private void OnDestroy()
    {
        GameManager.OnChangePlayers -= UpdatePlayerBoard;
    }

    private void UpdatePlayerBoard()
    {
        var players = GameManager.Players;

        for ( int i = 0; i < 4; i++ )
        {
            if ( i < players.Count )
            {
                boards[i].gameObject.SetActive( true );
                boards[i].UpdateInfomation( players[i] );
            }
            else
            {
                boards[i].gameObject.SetActive( false );
            }
        }

        UpdateOrderByKill( null ) ;
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
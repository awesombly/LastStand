using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TilemapManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private GameObject originTilemap;
    [SerializeField]
    private RuleTile ruleTile;
    [SerializeField]
    private int tilemapSize;

    private RuleTile.TilingRule tileRule;
    private Dictionary<int/*col*/, Dictionary<int/*row*/, GameObject/*tilemap*/>> tilemaps = new Dictionary<int, Dictionary<int, GameObject>>();

    private void Start()
    {
        tileRule = ruleTile.m_TilingRules[0];

        CreateTilemap( -1, -1 );
        CreateTilemap( -1, 0 );
        CreateTilemap( -1, 1 );
        CreateTilemap( 0, -1 );
        CreateTilemap( 0, 0 );
        CreateTilemap( 0, 1 );
        CreateTilemap( 1, -1 );
        CreateTilemap( 1, 0 );
        CreateTilemap( 1, 1 );
    }

    private void FixedUpdate()
    {
        UpdateTilemap();
    }

    private void CreateTilemap( int _col, int _row )
    {
        if ( !tilemaps.ContainsKey( _col ) )
        {
            tilemaps.Add( _col, new Dictionary<int, GameObject>() );
        }

        if ( tilemaps[_col].ContainsKey( _row ) )
        {
            return;
        }

        // 노이즈 조절
        tileRule.m_PerlinScale = Random.value;

        Vector2 position = new Vector2( _col * tilemapSize, _row * tilemapSize );
        GameObject newTilemap = Instantiate( originTilemap, position, Quaternion.identity, gameObject.transform );

        tilemaps[_col].Add( _row, newTilemap );
    }

    private void UpdateTilemap()
    {
        Vector2 pos = player.transform.position;
        float halfSize = tilemapSize * .5f;

        // 중심축 방향으로 이동시 차이가 발생해 보정치를 추가한다
        int tarCol = ( int )( pos.x + halfSize * Mathf.Sign( pos.x ) ) / tilemapSize;
        int tarRow = ( int )( pos.y + halfSize * Mathf.Sign( pos.y ) ) / tilemapSize;

        int x = Mathf.RoundToInt( player.Direction.x );
        int y = Mathf.RoundToInt( player.Direction.y );

        // 플레이어가 이동하는 방향 타일맵만 갱신
        if ( x != 0 )
        {
            CreateTilemap( tarCol + x, tarRow );
            CreateTilemap( tarCol + x, tarRow + 1 );
            CreateTilemap( tarCol + x, tarRow - 1 );
        }

        if ( y != 0 )
        {
            CreateTilemap( tarCol, tarRow + y );
            CreateTilemap( tarCol + 1, tarRow + y );
            CreateTilemap( tarCol - 1, tarRow + y );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Todo
// Ÿ�ϸʿ� Noise ����

public class TilemapManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private GameObject originTilemap;
    [SerializeField]
    private int tilemapSize;

    private Dictionary<int/*col*/, Dictionary<int/*row*/, GameObject/*tilemap*/>> tilemaps = new Dictionary<int, Dictionary<int, GameObject>>();

    private void Start()
    {
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

    private void CreateTilemap( int col, int row )
    {
        if ( tilemaps.ContainsKey( col ) == false )
        {
            tilemaps.Add( col, new Dictionary<int, GameObject>() );
        }

        if ( tilemaps[col].ContainsKey( row ) == true )
        {
            return;
        }

        Vector2 position = new Vector2( col * tilemapSize, row * tilemapSize );
        GameObject newTilemap = Instantiate( originTilemap, position, Quaternion.identity, gameObject.transform );

        tilemaps[col].Add( row, newTilemap );
    }

    private void UpdateTilemap()
    {
        Vector2 pos = player.transform.position;
        float halfSize = tilemapSize * .5f;

        // �߽��� �������� �̵��� ���̰� �߻��� ����ġ�� �߰��Ѵ�
        int tarCol = ( int )( pos.x + halfSize * Mathf.Sign( pos.x ) ) / tilemapSize;
        int tarRow = ( int )( pos.y + halfSize * Mathf.Sign( pos.y ) ) / tilemapSize;

        int x = Mathf.RoundToInt( player.Direction.x );
        int y = Mathf.RoundToInt( player.Direction.y );

        // �÷��̾ �̵��ϴ� ���� Ÿ�ϸʸ� ����
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

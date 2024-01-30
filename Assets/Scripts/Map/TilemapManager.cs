using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Todo
// 이동시 타일맵 생성/이동
// 타일맵에 Noise 적용
// 
// 퍼펙트픽셀카메라

public class TilemapManager : MonoBehaviour
{
    public GameObject originTilemap;
    public int tilemapSize;

    private Dictionary<int/*col*/, Dictionary<int/*row*/, GameObject/*tilemap*/>> tilemaps = new Dictionary<int, Dictionary<int, GameObject>>();

    void Start()
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

    private void CreateTilemap( int col, int row )
    {
        if ( tilemaps.ContainsKey( col ) == false )
        {
            tilemaps.Add( col, new Dictionary<int, GameObject>() );
        }

        if ( tilemaps[col].ContainsKey( row ) == true )
        {
            Debug.Log( "Exist tilemap, col = " + col + ", row = " + row );
            return;
        }

        Vector2 position = new Vector2( col * tilemapSize, row * tilemapSize );
        GameObject newTilemap = Instantiate( originTilemap, position, Quaternion.identity, gameObject.transform );

        tilemaps[col].Add( row, newTilemap );
    }
}

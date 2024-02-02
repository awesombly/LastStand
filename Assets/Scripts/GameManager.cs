using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Player player;
    
    [SerializeField]
    private float enemyRespawnTime;
    [SerializeField]
    private List<GameObject> enemies = new List<GameObject>();

    private float respawnTimer = 0f;

    private void Update()
    {
        respawnTimer += Time.deltaTime;

        if ( respawnTimer > enemyRespawnTime )
        {
            respawnTimer -= enemyRespawnTime;
            PoolObject obj = PoolManager.Inst.Get( enemies[0] );

            /// ToDo : 스폰 위치, 적 종류 설정
            obj.gameObject.transform.position = Vector3.zero;
        }
    }
}

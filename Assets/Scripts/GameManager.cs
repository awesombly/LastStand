using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Player player;

    [System.Serializable]
    protected struct EnemyInfo
    {
        public List<GameObject> prefabs;
        public float respawnTime;
        public float respawnDistance;
        [HideInInspector]
        public float respawnTimer;
    }
    [SerializeField]
    private EnemyInfo enemyInfo;

    private void Update()
    {
        SpawnEnemy( Time.deltaTime );
    }

    private void SpawnEnemy( float _deltaTime )
    {
        enemyInfo.respawnTimer += _deltaTime;
        if ( enemyInfo.respawnTimer > enemyInfo.respawnTime )
        {
            enemyInfo.respawnTimer -= enemyInfo.respawnTime;

            GameObject prefab = enemyInfo.prefabs[Random.Range( 0, enemyInfo.prefabs.Count )];
            PoolObject obj = PoolManager.Inst.Get( prefab );

            Vector3 delta = new Vector3( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ).normalized;
            Vector3 respawnPos = player.transform.position + delta * enemyInfo.respawnDistance;
            obj.gameObject.transform.position = respawnPos;
        }
    }
}

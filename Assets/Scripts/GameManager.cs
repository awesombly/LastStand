using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Player player;

    private int stageLevel;
    public int StageLevel
    {
        get { return stageLevel; }
        set
        {
            if ( value >= levelInfo.infos.Count )
            {
                Debug.Log( "StageLevel Overflow : " + value );
                stageLevel = 0;
                return;
            }
            stageLevel = value;
            Debug.Log( $"StageLevel = {stageLevel}" );
        }
    }

    private float levelTimer;

    [System.Serializable]
    protected class LevelInfo
    {
        public float levelInterval;
        public float spawnDistance;

        [System.Serializable]
        public class SpawnInfo
        {
            public GameObject prefab;
            public int count;
            public float delay;
            [HideInInspector]
            public float timer;
        }

        [System.Serializable]
        public class InfoPerLevels
        {
            public List<SpawnInfo> spawnInfos;
        }

        public List<InfoPerLevels> infos;
    }
    [SerializeField]
    private LevelInfo levelInfo;

    private void Update()
    {
        ProcessSpawnEnemy( Time.deltaTime );
        levelTimer += Time.deltaTime;
        if ( levelTimer >= levelInfo.levelInterval )
        {
            levelTimer -= levelInfo.levelInterval;
            ++StageLevel;
        }
    }

    private void ProcessSpawnEnemy( float _deltaTime )
    {
        LevelInfo.InfoPerLevels infoPerLevels = levelInfo.infos[StageLevel];

        for ( int i = 0; i < infoPerLevels.spawnInfos.Count; ++i )
        {
            LevelInfo.SpawnInfo spawnInfo = infoPerLevels.spawnInfos[i];

            spawnInfo.timer += _deltaTime;
            if ( spawnInfo.timer < spawnInfo.delay )
            {
                continue;
            }

            spawnInfo.timer -= spawnInfo.delay;

            for ( int count = 0; count < spawnInfo.count; ++count )
            {
                PoolObject poolObject = PoolManager.Inst.Get( spawnInfo.prefab );

                Vector3 delta = new Vector3( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ).normalized;
                Vector3 spawnPos = player.transform.position + delta * levelInfo.spawnDistance;
                poolObject.gameObject.transform.position = spawnPos;
            }
        }
    }
}

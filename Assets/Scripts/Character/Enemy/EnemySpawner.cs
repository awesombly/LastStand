using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> prefabs = new List<GameObject>();

    private Player player;
    private float startTime, timer;
    private float delay = 1f;

    private void Awake()
    {
        player = GameManager.Inst.player;
    }

    private void Start()
    {
        startTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        timer = Time.realtimeSinceStartup - startTime;
        if ( timer > delay )
        {
            startTime = Time.realtimeSinceStartup;
            Spawn();

            delay = Random.Range( .5f, 1f );
        }
    }

    private void Spawn()
    {
        Enemy obj = PoolManager.Inst.Get( prefabs[Random.Range( 0, prefabs.Count )] ) as Enemy;

        Vector2 delta = new Vector2( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ).normalized;
        obj.Initialize( player.Rigid2D.position + delta * 25f );
    }
}

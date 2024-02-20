using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<Poolable> prefabs = new List<Poolable>();

    private Player player;
    private float startTime, timer;
    private float delay = 1f;

    private void Awake()
    {
        player = GameManager.LocalPlayer;
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
        var prefab = prefabs[Random.Range( 0, prefabs.Count )];
        Vector2 delta = new Vector2( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ).normalized;

        ACTOR_INFO protocol;
        protocol.isLocal = false;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( prefab );
        protocol.serial = 0;
        protocol.pos = new VECTOR2( player.Rigid2D.position + delta * 25f );
        protocol.vel = new VECTOR2( Vector2.zero );
        Network.Inst.Send( PacketType.SPAWN_ACTOR_REQ, protocol );
    }
}

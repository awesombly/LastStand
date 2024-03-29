using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "GameManagerSO_", menuName = "Scriptable Objects/GameManagerSO" )]
public class GameManagerSO : ScriptableObject
{
    [Header( "< Respawn >" )]
    public float respawnDelay;
    public float respawnInvincibleTime;

    [Header( "< Prefab >" )]
    public Player playerPrefab;

    public PlayerSO pilotSO;
    public PlayerSO hunterSO;
    public PlayerSO convictSO;

    // PoolManager를 사용할 모든 프리팹들
    public List<Poolable/*Prefab*/> prefabList;
}

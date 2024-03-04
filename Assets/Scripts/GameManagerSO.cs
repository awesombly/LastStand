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
    public RuntimeAnimatorController pilotAC;
    public RuntimeAnimatorController hunterAC;
    public RuntimeAnimatorController convictAC;

    // PoolManager�� ����� ��� �����յ�
    public List<Poolable/*Prefab*/> prefabList;
}

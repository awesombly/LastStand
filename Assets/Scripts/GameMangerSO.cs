using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "GameManagerSO_", menuName = "Scriptable Objects/GameManagerSO" )]
public class GameMangerSO : ScriptableObject
{
    public Player playerPrefab;
    // PoolManager�� ����� ��� �����յ�
    public List<Poolable/*Prefab*/> prefabList;
}

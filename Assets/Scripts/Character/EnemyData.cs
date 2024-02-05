using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu( fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData" )]
public class EnemyData : ScriptableObject
{
    public float maxHp;
    public float moveSpeed;
    public float attackDamage;
    public LayerMask enemyArea;
}

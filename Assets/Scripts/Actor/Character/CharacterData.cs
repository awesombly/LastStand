using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( fileName = "CharacterData_", menuName = "Scriptable Objects/CharacterData" )]
public class CharacterData : ScriptableObject
{
    public float maxHp;
    public float moveSpeed;
    public float attackRate;
}

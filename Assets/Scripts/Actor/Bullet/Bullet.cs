using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Actor
{
    [Serializable]
    public struct EffectInfo
    {
        public GameObject fireEffect;
        public GameObject hitEffect;
        public GameObject shellEffect;
    }

    [SerializeField]
    private EffectInfo effect;

    [Serializable]
    public struct StatInfo
    {
        public float moveSpeed;
        public float damage;
        public float range;
        //[Range( 0f, 1f )]
        //public readonly float stability;
        public float pushingPower;
        public Global.StatusInt penetratePower;
    }
    [SerializeField]
    private StatInfo stat;
    private float lifeTime;

    [HideInInspector]
    public uint ownerSerial;
    private Global.LayerFlag targetLayer;
    private Rigidbody2D rigid2D;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if( !IsLocal )
        {
            return;
        }

        lifeTime -= Time.deltaTime;
        if ( lifeTime <= 0)
        {
            /// +제거 동기화
            Release();
        }
    }

    private void OnTriggerEnter2D( Collider2D _other )
    {
        if ( !gameObject.activeSelf )
        {
            return;
        }

        if ( !Global.CompareLayer( targetLayer, _other.gameObject.layer ) )
        {
            return;
        }

        Character attacker = GameManager.Inst.GetActor( ownerSerial ) as Character;
        Character otherCharacter = _other.GetComponent<Character>();
        otherCharacter?.OnHit( attacker, stat.damage, stat.pushingPower * transform.up );

        --stat.penetratePower.Current;
        if ( stat.penetratePower.IsZero )
        {
            /// + 제거 동기화
            Release();
        }
    }

    public void Init( uint _owner, Vector3 _position, Quaternion _rotation )
    {
        ownerSerial = _owner;
        targetLayer = IsLocal ? ( Global.LayerFlag.Enemy | Global.LayerFlag.Misc ) : 0;
        transform.SetPositionAndRotation( _position, _rotation );

        lifeTime = stat.range / stat.moveSpeed;
        stat.penetratePower.SetMax();
        rigid2D.velocity = transform.up * stat.moveSpeed;
    }
}

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
    public StatInfo stat;
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
        /// TODO: Release 동기화 하도록
        //if( !IsLocal )
        //{
        //    return;
        //}

        lifeTime -= Time.deltaTime;
        if ( lifeTime <= 0)
        {
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

        Character defender = _other.GetComponent<Character>();
        if ( defender == null )
        {
            Debug.LogWarning( "defender is null. name:" + _other.name );
            return;
        }

        --stat.penetratePower.Current;
        HIT_INFO protocol;
        protocol.needRelease = stat.penetratePower.IsZero;
        protocol.bullet = Serial;
        protocol.attacker = ownerSerial;
        protocol.defender = defender.Serial;
        Network.Inst.Send( PacketType.HIT_ACTOR_REQ, protocol );

        Character attacker = GameManager.Inst.GetActor( ownerSerial ) as Character;
        defender?.OnHit( attacker, stat.damage, stat.pushingPower * transform.up );
        
        if ( stat.penetratePower.IsZero )
        {
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

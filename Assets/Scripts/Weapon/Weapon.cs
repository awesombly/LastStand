using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private Bullet bulletPrefab;
    [SerializeField]
    private Transform shotPoint;
    [SerializeField]
    private float rotateCorrection;

    [Serializable]
    public struct StatInfo
    {
        public Global.StatusInt ammo;
        public Global.StatusInt magazine;
        public Global.StatusFloat repeatDelay;
        public Global.StatusFloat reloadDelay;
        public bool isAllowKeyHold;
        public float shakeShotAngle;
    }
    public StatInfo stat;

    [Serializable]
    public struct LookInfo
    {
        [HideInInspector]
        public float curAngle;
        [HideInInspector]
        public float targetAngle;
        public float lerpSpeed;
    }
    [SerializeField]
    private LookInfo lookInfo;

    private Character owner;
    private ActionReceiver receiver;

    #region Unity Callback
    private void Start()
    {
        if ( shotPoint == null )
        {
            shotPoint = transform;
        }

        stat.ammo.SetMax();
        stat.magazine.SetMax();
        stat.repeatDelay.SetZero();
        stat.reloadDelay.SetZero();
    }

    private void OnEnable()
    {
        owner = GetComponentInParent<Character>();
        receiver = GetComponentInParent<ActionReceiver>();
        receiver.OnAttackPressEvent += OnAttackPress;
        receiver.OnReloadEvent += OnReload;
        stat.reloadDelay.OnChangeCurrent += OnChangeReloadDelay;

        lookInfo.curAngle = lookInfo.targetAngle = transform.rotation.eulerAngles.z;
    }

    private void OnDisable()
    {
        receiver.OnAttackPressEvent -= OnAttackPress;
        receiver.OnReloadEvent -= OnReload;
        stat.reloadDelay.OnChangeCurrent -= OnChangeReloadDelay;
    }

    private void Update()
    {
        if ( !owner.IsLocal )
        {
            lookInfo.curAngle = Mathf.LerpAngle( lookInfo.curAngle, lookInfo.targetAngle, lookInfo.lerpSpeed * Time.deltaTime );
            transform.rotation = Quaternion.Euler( 0, 0, lookInfo.curAngle - 90 + rotateCorrection );
        }

        stat.repeatDelay.Current -= Time.deltaTime;
        stat.reloadDelay.Current -= Time.deltaTime;
        if ( stat.isAllowKeyHold && receiver.IsAttackHolded )
        {
            TryFire();
        }
    }
    #endregion

    public void LookAngle( bool _isImmediateChange, float _angle )
    {
        if ( owner.IsLocal || _isImmediateChange )
        {
            transform.rotation = Quaternion.Euler( 0, 0, _angle - 90 + rotateCorrection );
        }
        lookInfo.targetAngle = _angle;
        lookInfo.curAngle = transform.rotation.eulerAngles.z;
    }

    private void TryFire()
    {
        if ( !stat.repeatDelay.IsZero || !stat.reloadDelay.IsZero || owner.UnattackableCount > 0 )
        {
            return;
        }

        stat.repeatDelay.SetMax();
        if ( stat.magazine.IsZero )
        {
            OnReload();
            return;
        }

        --stat.magazine.Current;

        float angle = Global.GetAngle( shotPoint.position, GameManager.MouseWorldPos );
        angle += UnityEngine.Random.Range( -stat.shakeShotAngle * 0.5f, stat.shakeShotAngle * 0.5f );
        
        BULLET_INFO protocol;
        protocol.isLocal = false;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( bulletPrefab );
        protocol.serial = 0;
        protocol.pos = new VECTOR2( shotPoint.position );
        protocol.angle = angle;
        protocol.look = GameManager.LookAngle;
        protocol.owner = owner.Serial;
        protocol.damage = owner.data.attackRate * bulletPrefab.data.damage;
        Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, protocol );

        // 로컬 테스트용
        if ( !Network.Inst.IsConnected )
        {
            Bullet bullet = PoolManager.Inst.Get( bulletPrefab ) as Bullet;
            bullet.IsLocal = true;
            bullet.Fire( protocol );
            return;
        }
    }

    private void OnAttackPress()
    {
        if ( !stat.isAllowKeyHold )
        {
            TryFire();
        }
    }

    private void OnReload()
    {
        if ( stat.magazine.IsMax || stat.ammo.IsZero || stat.reloadDelay.Current > 0f )
        {
            return;
        }

        stat.reloadDelay.Max = Mathf.Max( stat.reloadDelay.Max, 0.001f );
        stat.reloadDelay.SetMax();
        SERIAL_INFO protocol;
        protocol.serial = owner.Serial;
        Network.Inst.Send( PacketType.SYNK_RELOAD_REQ, protocol );
    }

    private void OnChangeReloadDelay( float _old, float _new )
    {
        // 재장전 완료
        if ( stat.reloadDelay.IsZero )
        {
            int oldMag = stat.magazine.Current;
            stat.magazine.Current = Mathf.Clamp( stat.magazine.Current + stat.ammo.Current, 0, stat.magazine.Max );
            stat.ammo.Current -= ( stat.magazine.Current - oldMag );
        }
    }
}

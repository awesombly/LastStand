using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponSO data;

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
        public Global.StatusFloat swapDelay;
        public bool isAllowKeyHold;
        public float shakeShotAngle;
    }
    public StatInfo stat;

    [Serializable]
    public struct MultiShotInfo
    {
        [Min( 1 )]
        public int bulletPerShot;
        [Min( 0f )]
        public float spreadAngle;
        [Range( 0f, 1f )]
        public float speedRate;
        [Min( 1 )]
        public int burstCount;
        [Min( 0f )]
        public float burstDelay;
    }
    [SerializeField]
    public MultiShotInfo shotInfo;

    [Serializable]
    public struct LookInfo
    {
        [HideInInspector]
        public float curAngle;
        public float lerpSpeed;
    }
    [SerializeField]
    private LookInfo lookInfo;
    private Coroutine burstShotCoroutine;

    private Character owner;
    private ActionReceiver receiver;

    public event Action<Weapon> OnFire;
    public event Action<Weapon> OnReload;
    public event Action<Weapon> OnSwap;

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

    private void Update()
    {
        if ( !owner.IsLocal )
        {
            lookInfo.curAngle = Mathf.LerpAngle( lookInfo.curAngle, owner.LookAngle, lookInfo.lerpSpeed * Time.deltaTime );
            transform.rotation = Quaternion.Euler( 0, 0, lookInfo.curAngle - 90 + rotateCorrection );
        }

        stat.repeatDelay.Current -= Time.deltaTime;
        stat.reloadDelay.Current -= Time.deltaTime;
        stat.swapDelay.Current -= Time.deltaTime;
        if ( stat.isAllowKeyHold && receiver.IsAttackHolded )
        {
            TryFire();
        }
    }
    #endregion

    public void SetActiveWeapon( bool _isActive )
    {
        gameObject.SetActive( _isActive );
        if ( _isActive )
        {
            owner = GetComponentInParent<Character>( true );
            receiver = GetComponentInParent<ActionReceiver>( true );

            receiver.OnAttackPressEvent += OnAttackPress;
            receiver.OnReloadEvent += TryReload;
            stat.reloadDelay.OnChangeCurrent += OnChangeReloadDelay;

            lookInfo.curAngle = owner.LookAngle;
            transform.localScale = owner.IsFlipX ? new Vector3( -1f, -1f, 1f ) : Vector3.one;
            stat.swapDelay.SetMax();

            OnSwap?.Invoke( this );
        }
        else
        {
            receiver.OnAttackPressEvent -= OnAttackPress;
            receiver.OnReloadEvent -= TryReload;
            stat.reloadDelay.OnChangeCurrent -= OnChangeReloadDelay;
        }
    }

    public void LookAngle( bool _isImmediateChange, float _angle )
    {
        if ( owner.IsLocal || _isImmediateChange )
        {
            transform.rotation = Quaternion.Euler( 0, 0, _angle - 90 + rotateCorrection );
        }
        lookInfo.curAngle = transform.rotation.eulerAngles.z;
    }

    private void TryFire()
    {
        if ( !stat.repeatDelay.IsZero 
            || !stat.reloadDelay.IsZero 
            || !stat.swapDelay.IsZero 
            || owner.UnattackableCount > 0 )
        {
            return;
        }

        stat.repeatDelay.SetMax();
        if ( stat.magazine.IsZero )
        {
            TryReload();
            return;
        }

        if ( shotInfo.burstCount > 2 )
        {
            burstShotCoroutine = StartCoroutine( BurstShot( shotInfo.burstCount ) );
        }
        else
        {
            --stat.magazine.Current;
            OnFire?.Invoke( this );
            Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, MakeBulletShotInfo() );
        }
    }

    private IEnumerator BurstShot( int _burstCount )
    {
        while ( stat.magazine.Current > 0 && _burstCount > 0 )
        {
            --stat.magazine.Current;
            --_burstCount;
            OnFire?.Invoke( this );
            Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, MakeBulletShotInfo() );
            yield return YieldCache.WaitForSeconds( shotInfo.burstDelay );
        }

        burstShotCoroutine = null;
    }

    private BULLET_SHOT_INFO MakeBulletShotInfo()
    {
        float angle = Global.GetAngle( shotPoint.position, GameManager.MouseWorldPos );
        angle = GetRandomRange( angle, stat.shakeShotAngle );

        BULLET_SHOT_INFO protocol;
        protocol.isLocal = false;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( bulletPrefab );
        protocol.pos = new VECTOR2( shotPoint.position );
        protocol.look = GameManager.LookAngle;
        protocol.owner = owner.Serial;
        protocol.damage = owner.data.attackRate * bulletPrefab.data.damage;
        protocol.bullets = new List<BULLET_INFO>();

        for ( int i = 0; i < shotInfo.bulletPerShot; ++i )
        {
            var bullet = new BULLET_INFO();
            bullet.angle = GetRandomRange( angle, shotInfo.spreadAngle );
            bullet.angle = MathF.Round( bullet.angle, Global.RoundDigit );
            bullet.serial = 0;
            bullet.rate = UnityEngine.Random.Range( shotInfo.speedRate, 1f );
            bullet.rate = MathF.Round( bullet.rate, Global.RoundDigit );
            protocol.bullets.Add( bullet );
        }

        return protocol;
    }

    private float GetRandomRange( float _base, float _range )
    {
        return _base + UnityEngine.Random.Range( -_range * 0.5f, _range * 0.5f );
    }

    private void OnAttackPress()
    {
        if ( !stat.isAllowKeyHold )
        {
            TryFire();
        }
    }

    private void TryReload()
    {
        if ( stat.magazine.IsMax
            || stat.ammo.IsZero
            || stat.reloadDelay.Current > 0f
            || !ReferenceEquals( burstShotCoroutine, null ) )
        {
            return;
        }

        stat.reloadDelay.Max = Mathf.Max( stat.reloadDelay.Max, 0.001f );
        stat.reloadDelay.SetMax();
        OnReload?.Invoke( this );

        SERIAL_INFO protocol;
        protocol.serial = owner.Serial;
        Network.Inst.Send( PacketType.SYNC_RELOAD_REQ, protocol );
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

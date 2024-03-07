using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponSO data;
    public SpriteRenderer spriter;
    [SerializeField]
    private Transform shotPoint;
    [SerializeField]
    private float rotateCorrection;
    [SerializeField]
    private float rotateLerpSpeed;

    public struct StatInfo
    {
        public Global.StatusInt ammo;
        public Global.StatusInt magazine;
        public Global.StatusFloat repeatDelay;
        public Global.StatusFloat reloadDelay;
        public Global.StatusFloat swapDelay;
    }
    [HideInInspector]
    public StatInfo myStat;

    private float curLookAngle;
    private Coroutine burstShotCoroutine;

    private Character owner;
    private ActionReceiver receiver;

    public Action<Weapon> OnFireEvent;
    public event Action<Weapon> OnReloadEvent;
    public event Action<Weapon> OnSwapEvent;

    #region Unity Callback
    private void Awake()
    {
        if ( shotPoint is null )
        {
            shotPoint = transform;
        }

        myStat.ammo.Max = data.stat.ammo;
        myStat.magazine.Max = data.stat.magazine;
        myStat.repeatDelay.Max = data.stat.repeatDelay;
        myStat.reloadDelay.Max = data.stat.reloadDelay;
        myStat.swapDelay.Max = data.stat.swapDelay;

        myStat.ammo.SetMax();
        myStat.magazine.SetMax();
        myStat.repeatDelay.SetZero();
        myStat.reloadDelay.SetZero();
    }

    private void Update()
    {
        if ( !owner.IsLocal && !owner.IsOnFire )
        {
            curLookAngle = Mathf.LerpAngle( curLookAngle, owner.LookAngle, rotateLerpSpeed * Time.deltaTime );
            transform.rotation = Quaternion.Euler( 0, 0, curLookAngle - 90 + rotateCorrection );
        }

        myStat.repeatDelay.Current -= Time.deltaTime;
        myStat.reloadDelay.Current -= Time.deltaTime;
        myStat.swapDelay.Current -= Time.deltaTime;
        if ( data.stat.isAllowKeyHold && receiver.IsAttackHolded )
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
            myStat.reloadDelay.OnChangeCurrent += OnChangeReloadDelay;

            curLookAngle = owner.LookAngle;
            transform.localScale = owner.IsFlipX ? new Vector3( -1f, -1f, 1f ) : Vector3.one;
            myStat.swapDelay.SetMax();

            OnSwapEvent?.Invoke( this );
        }
        else
        {
            receiver.OnAttackPressEvent -= OnAttackPress;
            receiver.OnReloadEvent -= TryReload;
            myStat.reloadDelay.OnChangeCurrent -= OnChangeReloadDelay;
        }
    }

    public void LookAngle( bool _isImmediateChange, float _angle )
    {
        if ( !owner.IsOnFire && ( owner.IsLocal || _isImmediateChange ) )
        {
            transform.rotation = Quaternion.Euler( 0, 0, _angle - 90 + rotateCorrection );
        }
        curLookAngle = transform.rotation.eulerAngles.z;
    }

    public void InvokeOnFire()
    {
        OnFireEvent?.Invoke( this );

        float radian = owner.LookAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2( Mathf.Cos( radian ), Mathf.Sin( radian ) );
        owner.Rigid2D.AddForce( -direction * owner.EquipWeapon.data.stat.reactionPower );
    }

    private void TryFire()
    {
        if ( !myStat.repeatDelay.IsZero 
            || !myStat.reloadDelay.IsZero 
            || !myStat.swapDelay.IsZero 
            || owner.UnactionableCount > 0 )
        {
            return;
        }

        myStat.repeatDelay.SetMax();
        if ( myStat.magazine.IsZero )
        {
            TryReload();
            return;
        }

        if ( data.shotInfo.burstCount > 2 )
        {
            burstShotCoroutine = StartCoroutine( BurstShot( data.shotInfo.burstCount ) );
        }
        else
        {
            --myStat.magazine.Current;
            Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, MakeBulletShotInfo() );
        }
    }

    private IEnumerator BurstShot( int _burstCount )
    {
        while ( myStat.magazine.Current > 0 && _burstCount > 0 )
        {
            --myStat.magazine.Current;
            --_burstCount;
            Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, MakeBulletShotInfo() );
            yield return YieldCache.WaitForSeconds( data.shotInfo.burstDelay );
        }

        burstShotCoroutine = null;
    }

    private BULLET_SHOT_INFO MakeBulletShotInfo()
    {
        float angle = Global.GetAngle( shotPoint.position, GameManager.MouseWorldPos );
        angle = GetRandomRange( angle, data.stat.shakeShotAngle );

        BULLET_SHOT_INFO protocol;
        protocol.isLocal = false;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( data.bulletPrefab );
        protocol.pos = new VECTOR2( shotPoint.position );
        protocol.look = GameManager.LookAngle;
        protocol.owner = owner.Serial;
        protocol.damage = owner.data.attackRate * data.bulletPrefab.data.damage;
        protocol.hp = data.bulletPrefab.data.penetratePower * 20f;
        protocol.bullets = new List<BULLET_INFO>();

        for ( int i = 0; i < data.shotInfo.bulletPerShot; ++i )
        {
            var bullet = new BULLET_INFO();
            bullet.angle = GetRandomRange( angle, data.shotInfo.spreadAngle );
            bullet.angle = MathF.Round( bullet.angle, Global.RoundDigit );
            bullet.serial = 0;
            bullet.rate = UnityEngine.Random.Range( data.shotInfo.speedRate, 1f );
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
        if ( !data.stat.isAllowKeyHold )
        {
            TryFire();
        }
    }

    private void TryReload()
    {
        if ( myStat.magazine.IsMax
            || myStat.ammo.IsZero
            || myStat.reloadDelay.Current > 0f
            || !ReferenceEquals( burstShotCoroutine, null ) )
        {
            return;
        }

        myStat.reloadDelay.SetMax();
        OnReloadEvent?.Invoke( this );

        SERIAL_INFO protocol;
        protocol.serial = owner.Serial;
        Network.Inst.Send( PacketType.SYNC_RELOAD_REQ, protocol );
    }

    private void OnChangeReloadDelay( float _old, float _new, float _max )
    {
        // 재장전 완료
        if ( myStat.reloadDelay.IsZero )
        {
            int oldMag = myStat.magazine.Current;
            myStat.magazine.Current = Mathf.Clamp( myStat.magazine.Current + myStat.ammo.Current, 0, myStat.magazine.Max );
            myStat.ammo.Current -= ( myStat.magazine.Current - oldMag );
        }
    }
}

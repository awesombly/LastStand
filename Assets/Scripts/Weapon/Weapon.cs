using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    #region Base
    [Header( "─ Base" )]
    [SerializeField]
    private Bullet bulletPrefab;
    [SerializeField]
    private Transform shotPoint;
    [SerializeField]
    private float rotateCorrection;
    #endregion

    #region Weapon Stat
    [Header( "─ Weapon Stat" )]
    [SerializeField]
    private Global.StatusInt ammo;
    [SerializeField]
    private Global.StatusInt magazine;
    [SerializeField]
    private Global.StatusFloat repeatDelay;
    public Global.StatusFloat reloadDelay;
    [SerializeField]
    private bool isAllowKeyHold;
    [SerializeField]
    private float shakeShotAngle;
    #endregion

    #region Use Not Local
    private float curLookAngle;
    private float targetLookAngle;
    [SerializeField]
    private float lookAngleLerpSpeed;
    #endregion

    #region UI
    [Header( "─ UI" )]
    [SerializeField]
    private TextMeshProUGUI magazineUI;
    [SerializeField]
    private TextMeshProUGUI ammoUI;
    [SerializeField]
    private Slider reloadBar;
    #endregion

    private Character owner;
    private ActionReceiver receiver;

    #region Unity Callback
    private void Start()
    {
        if ( shotPoint == null )
        {
            shotPoint = transform;
        }

        ammo.SetMax();
        magazine.SetMax();
        repeatDelay.SetZero();
        reloadDelay.SetZero();
    }

    private void OnEnable()
    {
        owner = GetComponentInParent<Character>();
        receiver = GetComponentInParent<ActionReceiver>();
        receiver.OnAttackPressEvent += OnAttackPress;
        receiver.OnReloadEvent += OnReload;

        //ammo.OnChangeCurrent += OnChangeAmmo;
        ammoUI?.SetText( magazine.Max.ToString() );
        reloadBar.gameObject.SetActive( false );
        magazine.OnChangeCurrent += OnChangeMagazine;
        reloadDelay.OnChangeCurrent += OnChangeReloadDelay;

        curLookAngle = targetLookAngle = transform.rotation.eulerAngles.z;
    }

    private void OnDisable()
    {
        receiver.OnAttackPressEvent -= OnAttackPress;
        receiver.OnReloadEvent -= OnReload;

        //ammo.OnChangeCurrent -= OnChangeAmmo;
        magazine.OnChangeCurrent -= OnChangeMagazine;
        reloadDelay.OnChangeCurrent -= OnChangeReloadDelay;
    }

    private void Update()
    {
        if ( !owner.IsLocal )
        {
            curLookAngle = Mathf.LerpAngle( curLookAngle, targetLookAngle, lookAngleLerpSpeed * Time.deltaTime );
            transform.rotation = Quaternion.Euler( 0, 0, curLookAngle - 90 + rotateCorrection );
        }

        repeatDelay.Current -= Time.deltaTime;
        reloadDelay.Current -= Time.deltaTime;
        if ( isAllowKeyHold && receiver.IsAttackHolded && repeatDelay.IsZero && reloadDelay.IsZero )
        {
            Fire();
        }
    }
    #endregion

    public void LookAngle( bool _isImmediateChange, float _angle )
    {
        if ( owner.IsLocal || _isImmediateChange )
        {
            transform.rotation = Quaternion.Euler( 0, 0, _angle - 90 + rotateCorrection );
        }
        targetLookAngle = _angle;
        curLookAngle = transform.rotation.eulerAngles.z;
    }

    private void Fire()
    {
        repeatDelay.SetMax();
        if ( magazine.IsZero )
        {
            OnReload();
            return;
        }

        --magazine.Current;

        float angle = Global.GetAngle( shotPoint.position, GameManager.MouseWorldPos );
        angle += Random.Range( -shakeShotAngle * 0.5f, shakeShotAngle * 0.5f );
        
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
            bullet.Init( protocol );
            return;
        }
    }

    private void OnAttackPress()
    {
        if ( !isAllowKeyHold && repeatDelay.IsZero && reloadDelay.IsZero )
        {
            Fire();
        }
    }

    private void OnReload()
    {
        if ( magazine.IsMax || ammo.IsZero || reloadDelay.Current > 0f )
        {
            return;
        }

        reloadDelay.Max = Mathf.Max( reloadDelay.Max, 0.001f );
        reloadDelay.SetMax();
        SERIAL_INFO protocol;
        protocol.serial = owner.Serial;
        Network.Inst.Send( PacketType.SYNK_RELOAD_REQ, protocol );
    }

    private void OnChangeAmmo( int _old, int _new )
    {
        ammoUI?.SetText( _new.ToString() );
    }

    private void OnChangeMagazine( int _old, int _new )
    {
        magazineUI?.SetText( _new.ToString() );
    }

    private void OnChangeReloadDelay( float _old, float _new )
    {
        // 재장전 완료
        if ( reloadDelay.IsZero )
        {
            int oldMag = magazine.Current;
            magazine.Current = Mathf.Clamp( magazine.Current + ammo.Current, 0, magazine.Max );
            ammo.Current -= ( magazine.Current - oldMag );
        }

        reloadBar.gameObject.SetActive( !reloadDelay.IsZero );
        reloadBar.value = ( reloadDelay.Max - reloadDelay.Current ) / reloadDelay.Max;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform shotPoint;

    [SerializeField]
    private Global.StatusInt ammo;
    [SerializeField]
    private Global.StatusInt magazine;
    [SerializeField]
    private Global.StatusFloat repeatDelay;
    [SerializeField]
    private Global.StatusFloat reloadDelay;
    [SerializeField]
    private bool isAllowKeyHold;

    private Character owner;
    private ActionReceiver receiver;

    #region UI
    [SerializeField]
    private TextMeshProUGUI magazineUI;
    [SerializeField]
    private TextMeshProUGUI ammoUI;
    [SerializeField]
    private Slider reloadBar;
    #endregion

    #region Unity Callback
    private void Start()
    {
        if ( shotPoint == null )
        {
            shotPoint = transform;
        }

        ammo.SetMax();
        magazine.SetMax();
    }

    private void OnEnable()
    {
        owner = GetComponentInParent<Character>();
        receiver = GetComponentInParent<ActionReceiver>();
        receiver.OnAttackPressEvent += OnAttackPress;
        receiver.OnReloadEvent += OnReload;

        ammo.OnChangeCurrent += OnChangeAmmo;
        magazine.OnChangeCurrent += OnChangeMagazine;
        reloadDelay.OnChangeCurrent += OnChangeReloadDelay;
    }

    private void OnDisable()
    {
        receiver.OnAttackPressEvent -= OnAttackPress;
        receiver.OnReloadEvent -= OnReload;

        ammo.OnChangeCurrent -= OnChangeAmmo;
        magazine.OnChangeCurrent -= OnChangeMagazine;
        reloadDelay.OnChangeCurrent -= OnChangeReloadDelay;
    }

    private void Update()
    {
        repeatDelay.Current -= Time.deltaTime;
        reloadDelay.Current -= Time.deltaTime;
        if ( isAllowKeyHold && receiver.IsAttackHolded && repeatDelay.IsZero && reloadDelay.IsZero )
        {
            Fire();
        }
    }
    #endregion

    private void Fire()
    {
        repeatDelay.SetMax();
        if ( magazine.IsZero )
        {
            OnReload();
            return;
        }

        --magazine.Current;
        Debug.Log( $"mag:{magazine.Current}, ammo:{ammo.Current}" );
        Vector3 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );

        Vector3 dir = ( mousePos - shotPoint.position ).normalized;
        float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler( 0, 0, angle - 90 );

        // 로컬 테스트용
        if ( !Network.Inst.IsConnected )
        {
            Bullet bullet = PoolManager.Inst.Get( bulletPrefab ) as Bullet;
            bullet.IsLocal = true;
            bullet.Init( owner.Serial, shotPoint.position, rotation );
            return;
        }

        BULLET_INFO protocol;
        protocol.actorInfo.isLocal = false;
        protocol.actorInfo.prefab = GameManager.Inst.GetPrefabIndex( bulletPrefab );
        protocol.actorInfo.serial = 0;
        protocol.actorInfo.position = new VECTOR3( shotPoint.position );
        protocol.actorInfo.rotation = new QUATERNION( rotation );
        protocol.actorInfo.velocity = new VECTOR3( Vector3.zero );
        protocol.owner = owner.Serial;
        Network.Inst.Send( PacketType.SPAWN_BULLET_REQ, protocol );
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

        reloadDelay.SetMax();

        int oldMag = magazine.Current;
        magazine.Current = Mathf.Clamp( magazine.Current + ammo.Current, 0, magazine.Max );
        ammo.Current -= ( magazine.Current - oldMag );

        Debug.Log( $"Reload, mag:{magazine.Current}, ammo:{ammo.Current}" );
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
        reloadBar.value = ( reloadDelay.Max - reloadDelay.Current ) / reloadDelay.Max;
    }
}

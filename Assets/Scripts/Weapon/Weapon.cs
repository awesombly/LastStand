using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    }

    private void OnDisable()
    {
        receiver.OnAttackPressEvent -= OnAttackPress;
        receiver.OnReloadEvent -= OnReload;
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

        Bullet bullet = PoolManager.Inst.Get( bulletPrefab ) as Bullet;
        bullet.owner = owner;
        bullet.targetLayer = Global.LayerValue.Enemy | Global.LayerValue.Misc;
        bullet.transform.position = shotPoint.position;

        Vector3 dir = ( mousePos - shotPoint.position ).normalized;
        float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler( 0, 0, angle - 90 );

        bullet.Fire();
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
}

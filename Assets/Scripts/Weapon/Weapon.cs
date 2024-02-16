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

    private uint ownerSerial = 0;
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

        Vector3 dir = ( mousePos - shotPoint.position ).normalized;
        float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler( 0, 0, angle - 90 );

        ACTOR_INFO protocol;
        protocol.isLocal = false;
        protocol.prefab = GameManager.Inst.GetPrefabIndex( bulletPrefab );
        protocol.serial = ownerSerial;
        protocol.position = new VECTOR3( shotPoint.position );
        protocol.rotation = new QUATERNION( rotation );
        protocol.velocity = new VECTOR3( Vector3.zero );
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
}

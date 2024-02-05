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

    private void Start()
    {
        if ( shotPoint == null )
        {
            shotPoint = transform;
        }

        GameManager.Inst.player.OnAttackEvent += OnAttack;
    }

    private void OnAttack( InputValue _value )
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );

        PoolObject bulletObj = PoolManager.Inst.Get( bulletPrefab );
        bulletObj.transform.position = shotPoint.position;

        Vector3 dir = ( mousePos - shotPoint.position ).normalized;
        float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
        bulletObj.transform.rotation = Quaternion.Euler( 0, 0, angle - 90 );
        
        var bullet = bulletObj.GetComponent<Bullet>();
        bullet?.Fire();
    }
}

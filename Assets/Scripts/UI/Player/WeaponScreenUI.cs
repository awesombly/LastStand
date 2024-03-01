using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponScreenUI : MonoBehaviour
{
    [SerializeField]
    private Image weaponImage;

    [SerializeField]
    private GameObject magazinePanel;
    [SerializeField]
    private Image bulletIconPrefab;
    private List<Image> bulletIcons = new List<Image>();
    private WNS.ObjectPool<Image> bulletIconPool;

    private Weapon equipWeapon;

    private void Awake()
    {
        bulletIconPool = new WNS.ObjectPool<Image>( bulletIconPrefab, magazinePanel.transform );

        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;
    }

    private void UpdateBulletIcons()
    {
        if ( ReferenceEquals( equipWeapon, null ) )
        {
            return;
        }

        int usedMagazine = equipWeapon.myStat.magazine.Max - equipWeapon.myStat.magazine.Current;
        for ( int i = 0; i < bulletIcons.Count; ++i )
        {
            if ( usedMagazine <= i )
            {
                bulletIcons[i].color = Color.white;
            }
            else
            {
                bulletIcons[i].color = Color.gray;
            }
        }
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( !ReferenceEquals( _old, null ) )
        {
            _old.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }

        if ( !ReferenceEquals( _new, null ) )
        {
            _new.OnChangeEquipWeapon += OnChangeEquipWeapon;
        }
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( !ReferenceEquals( _old, null ) )
        {
            _old.myStat.magazine.OnChangeCurrent -= OnChangeMagazine;
        }

        if ( ReferenceEquals( _new, null ) )
        {
            return;
        }
        equipWeapon = _new;
        equipWeapon.myStat.magazine.OnChangeCurrent += OnChangeMagazine;

        // Weapon Image
        weaponImage.sprite = equipWeapon.spriter.sprite;

        // Bullet Icons
        bulletIcons.Clear();
        bulletIconPool.AllDespawn();

        for ( int i = 0; i < equipWeapon.myStat.magazine.Max; ++i )
        {
            Image bullet = bulletIconPool.Spawn();
            bullet.transform.SetSiblingIndex( i );
            bulletIcons.Add( bullet );
        }
        UpdateBulletIcons();
        // 처음 접속시 업데이트 안되는 이슈가 있어 추가
        LayoutRebuilder.ForceRebuildLayoutImmediate( magazinePanel.GetComponent<RectTransform>() );
    }

    private void OnChangeMagazine( int _old, int _new, int _max )
    {
        UpdateBulletIcons();
    }
}

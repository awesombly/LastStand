using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

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

    // Ammo Effect
    public GameObject ammo;
    private Transform ammoTF;
    private RectTransform ammoRT;

    private Sequence moveEffectSeq;
    private Vector2 startPos, endPos;

    private Sequence scaleEffectSeq;
    private Vector3 startScl, endScl;


    private void Awake()
    {
        bulletIconPool = new WNS.ObjectPool<Image>( bulletIconPrefab, magazinePanel.transform );

        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;
    }

    private void Start()
    {
        AmmoEffectInitialize();
    }

    private void AmmoEffectInitialize()
    {
        // Scale Effect
        ammoTF   = ammo.transform;
        startScl = ammoTF.localScale;
        endScl   = startScl * 1.15f;

        scaleEffectSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        scaleEffectSeq.AppendCallback( () => ammoTF.localScale = startScl )
                      .Append( ammoTF.DOScale( endScl, .15f ) )
                      .OnComplete( () => ammoTF.DOScale( startScl, .15f ) );

        // Move Effect
        ammoRT   = ammoTF as RectTransform;
        startPos = ammoRT.anchoredPosition;
        endPos   = new Vector2( startPos.x, startPos.y + 5f );

        moveEffectSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        moveEffectSeq.AppendCallback( () => ammoRT.anchoredPosition = startPos )
                     .Append( ammoRT.DOAnchorPos( endPos, .1f ) )
                     .OnComplete( () => ammoRT.DOAnchorPos( startPos, .1f ) );
    }

    private void PlayEffect( Weapon _w )
    {
        moveEffectSeq.Restart();
        scaleEffectSeq.Restart();
    }

    private void OnDestroy()
    {
        GameManager.OnChangeLocalPlayer -= OnChangeLocalPlayer;
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
            _old.OnFireEvent -= PlayEffect;
            _old.myStat.magazine.OnChangeCurrent -= OnChangeMagazine;
        }

        if ( ReferenceEquals( _new, null ) )
             return;

        equipWeapon = _new;
        equipWeapon.OnFireEvent                     += PlayEffect;
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
        // ó�� ���ӽ� ������Ʈ �ȵǴ� �̽��� �־� �߰�
        LayoutRebuilder.ForceRebuildLayoutImmediate( magazinePanel.GetComponent<RectTransform>() );
    }

    private void OnChangeMagazine( int _old, int _new, int _max )
    {
        UpdateBulletIcons();
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VirtualPadUI : MonoBehaviour
{
    [SerializeField] Image reloadImage;
    [SerializeField] Image dodgeImage;
    [SerializeField] Image interactionImage;
    [SerializeField] Image prevWeaponImage;
    [SerializeField] Image nextWeaponImage;

    private Player targetPlayer = null;

    private void Awake()
    {
        GameManager.OnChangeLocalPlayer += OnChangeLocalPlayer;
    }

    private void OnDestroy()
    {
        GameManager.OnChangeLocalPlayer -= OnChangeLocalPlayer;

        if ( targetPlayer is not null )
        {
            targetPlayer.OnChangePlayerType -= OnChangePlayerType;
            targetPlayer.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }
    }

    private void OnChangeLocalPlayer( Player _old, Player _new )
    {
        if ( _old == _new )
        {
            return;
        }

        if ( _old is not null )
        {
            _old.OnChangePlayerType -= OnChangePlayerType;
            _old.OnChangeEquipWeapon -= OnChangeEquipWeapon;
        }

        if ( _new is not null )
        {
            targetPlayer = _new;
            _new.OnChangePlayerType += OnChangePlayerType;
            _new.OnChangeEquipWeapon += OnChangeEquipWeapon;
        }
    }

    private void OnChangePlayerType( PlayerType _type )
    {
        PlayerSO data = GameManager.Inst.GetPlayerSO( _type );

        reloadImage.sprite = data.pad.reloadImage;
        dodgeImage.sprite = data.pad.dodgeImage;
        interactionImage.sprite = data.pad.interactionImage;
    }

    private void OnChangeEquipWeapon( Weapon _old, Weapon _new )
    {
        if ( _old == _new || _new is null )
        {
            return;
        }

        prevWeaponImage.sprite = targetPlayer.PrevWeapon.spriter.sprite;
        nextWeaponImage.sprite = targetPlayer.NextWeapon.spriter.sprite;
    }
}

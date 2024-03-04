using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

using static PacketType;
public class Stage : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI curPersonnel;
    public TextMeshProUGUI maxPersonnel;

    public STAGE_INFO info;

    private RectTransform rt;

    private void Awake()
    {
        rt = transform as RectTransform;
    }

    public void Initialize( STAGE_INFO _info )
    {
        info = _info;
        transform.localScale = Vector3.one;

        title.text        = info.title;
        maxPersonnel.text = $"{info.personnel.maximum}";
        curPersonnel.text = $"{info.personnel.current}";
    }

    public void EntryStage()
    {
        if ( SceneBase.IsLock )
             return;

        SceneBase.IsLock = true;
        Network.Inst.Send( new Packet( ENTRY_STAGE_REQ, info ) );
    }

    public void OnPointerEnter()
    {
        AudioManager.Inst.Play( SFX.MenuHover );
        rt.DOSizeDelta( new Vector2( 800f, 125f ), .35f );
    }

    public void OnPointerExit()
    {
        rt.DOSizeDelta( new Vector2( 750f, 100f ), .35f );
    }
}
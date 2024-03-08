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
    public TextMeshProUGUI targetKill;

    public STAGE_INFO info;

    public void Initialize( STAGE_INFO _info )
    {
        info = _info;
        transform.localScale = Vector3.one;

        title.text        = info.title;
        maxPersonnel.text = $"{info.personnel.maximum}";
        curPersonnel.text = $"{info.personnel.current}";
        targetKill.text   = $"{info.targetKill}";
    }

    public void EntryStage()
    {
        if ( SceneBase.IsLock )
             return;

        SceneBase.IsLock = true;
        Network.Inst.Send( new Packet( ENTRY_STAGE_REQ, info ) );
    }
}
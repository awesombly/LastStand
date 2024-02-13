using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class Stage : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI curPersonnel;
    public TextMeshProUGUI maxPersonnel;

    public STAGE_INFO info;

    public void Initialize( STAGE_INFO _info )
    {
        info = _info;

        title.text        = info.title;
        maxPersonnel.text = $"{info.personnel.maximum}";
        curPersonnel.text = $"{info.personnel.current}";
    }

    public void EntryStage()
    {
        Network.Inst.Send( new Packet( ENTRY_STAGE_REQ, info ) );
    }
}
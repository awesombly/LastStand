using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI curPersonnel;
    public TextMeshProUGUI maxPersonnel;

    public ROOM_INFO info;

    public void Initialize( ROOM_INFO _info )
    {
        info = _info;

        title.text        = info.title;
        maxPersonnel.text = $"{info.personnel.maximum}";
        curPersonnel.text = $"{info.personnel.current}";
    }
}

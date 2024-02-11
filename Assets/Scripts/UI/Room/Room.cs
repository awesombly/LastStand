using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI curPersonnel;
    public TextMeshProUGUI maxPersonnel;

    public RoomData data;

    public void Initialize( RoomData _data )
    {
        data = _data;

        title.text        = data.title;
        maxPersonnel.text = data.maxPersonnel.ToString();
        curPersonnel.text = 0.ToString();
    }
}

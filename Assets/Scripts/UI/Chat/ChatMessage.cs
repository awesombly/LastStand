using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatMessage : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public void Initialize( CHAT_MESSAGE _message )
    {
        transform.SetAsLastSibling();
        text.text = $"<{_message.nickname}>  {_message.message}";
        text.alpha = 1f;
    }
}
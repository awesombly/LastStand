using TMPro;
using UnityEngine;

public class ChatSystem : MonoBehaviour
{
    public TextMeshProUGUI prefab;
    public TMP_InputField  input;
    public Transform       contents;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( new ChatMessage(), PrintMessage );

        input.interactable = false;
    }

    private void PrintMessage( Packet _packet )
    {
        var text  = Instantiate( prefab, contents );
        text.text = System.Text.Encoding.UTF8.GetString( _packet.data );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            if ( input.interactable )
            {
                input.interactable = false;
                input.DeactivateInputField();

                ChatMessage message;
                message.message = input.text;

                Network.Inst.Send( new Packet( message ) );

                input.text = string.Empty;
            }
            else
            {
                input.interactable = true;
                input.ActivateInputField();
            }
        }
    }
}
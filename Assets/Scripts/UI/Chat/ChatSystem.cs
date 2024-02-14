using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static PacketType;
public class ChatSystem : MonoBehaviour
{
    public ChatMsg prefab;
    public TMP_InputField  input;
    public Transform       contents;
    public WNS.ObjectPool<ChatMsg> pool;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( PACKET_CHAT_MSG, PrintMessage );

        pool = new WNS.ObjectPool<ChatMsg>( prefab, 5 );
        input.interactable = false;
    }

    private void PrintMessage( Packet _packet )
    {
        var data = Global.FromJson<CHAT_MESSAGE>( _packet );
        var obj  = pool.Spawn( contents );
        obj.Initialize( data );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            if ( input.interactable )
            {
                input.interactable = false;
                input.DeactivateInputField();

                CHAT_MESSAGE message;
                message.nickname = LoginSystem.Info.nickname;
                message.message  = input.text;
                Network.Inst.Send( PACKET_CHAT_MSG, message );

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
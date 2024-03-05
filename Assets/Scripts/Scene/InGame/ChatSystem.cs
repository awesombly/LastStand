using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class ChatSystem : MonoBehaviour
{
    public ChatMessage prefab;
    private WNS.ObjectPool<ChatMessage> pool;

    public TMP_InputField  input;
    public Transform       contents;
    private LinkedList<ChatMessage> msgList = new LinkedList<ChatMessage>();
    private readonly int MaxMesaageCount = 8;

    private void Awake()
    {
        ProtocolSystem.Inst.Regist( PACKET_CHAT_MSG, AckBroadcastMessage );

        pool = new WNS.ObjectPool<ChatMessage>( prefab, contents );
        input.interactable = false;
    }

    private void AckBroadcastMessage( Packet _packet )
    {
        CHAT_MESSAGE data = Global.FromJson<CHAT_MESSAGE>( _packet );

        ChatMessage msg  = pool.Spawn();
        msg.Initialize( data );
        msgList.AddLast( msg );
        if ( msgList.Count > MaxMesaageCount )
        {
            pool.Despawn( msgList.First.Value );
            msgList.RemoveFirst();
        }

        var player = GameManager.Inst.GetActor( data.serial ) as Player;
        if ( !ReferenceEquals( player, null ) )
        {
            player.ReceiveMessage( data.message );
        }
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            if ( input.interactable )
            {
                input.interactable = false;
                input.DeactivateInputField();
                if ( !ReferenceEquals( GameManager.LocalPlayer, null ) )
                {
                    GameManager.LocalPlayer.UnmoveableCount--;
                }

                CHAT_MESSAGE message;
                message.serial   = GameManager.LocalPlayer.Serial;
                message.nickname = GameManager.LoginInfo == null ? string.Empty : GameManager.LoginInfo.Value.nickname;
                message.message  = input.text;
                Network.Inst.Send( PACKET_CHAT_MSG, message );

                input.text = string.Empty;
            }
            else
            {
                input.interactable = true;
                input.ActivateInputField();
                if ( !ReferenceEquals( GameManager.LocalPlayer, null ) )
                {
                    GameManager.LocalPlayer.UnmoveableCount++;
                }
            }
        }
    }
}
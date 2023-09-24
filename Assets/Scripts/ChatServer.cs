using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if(IsServer)
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if (IsHost) 
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            } else
            {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
        } else
        {
            DisplayMessageLocally(SYSTEM_ID, $"You are client {NetworkManager.LocalClientId}");
            DisplayMessageLocally(SYSTEM_ID, $"Welcome to the server Player {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        SendChatMessageServerRpc(
            $"Welcome Player {clientId} to the server");
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        SendChatMessageServerRpc(
            $"We will miss you Player {clientId}");
    }

    private void DisplayMessageLocally(ulong from, string message)
    {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if (from == NetworkManager.LocalClientId)
        {
            fromStr = "You";
            textColor = Color.magenta;
        }else if(from == SYSTEM_ID)
        {
            fromStr = "SYS";
            textColor = Color.red;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);

            ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);   
        } else
        {
            RecieveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void RecieveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        //clientIds[0] = from;
        //RecieveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);

        //clientIds[0] = to;
        RecieveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
        if (to != dmClientIds[1])
        {
            BadIdMessage(from, message);
        }
    }

    private void BadIdMessage(ulong from, string message)
    {
        ServerSendDirectMessage("Message could not be sent because client does not exist", SYSTEM_ID, from);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkHandler : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void PrintMe()
    {
        if (IsServer)
        {
            NetworkHelper.Log($"I AM a Server! {NetworkManager.ServerClientId}");
        }
        if (IsHost)
        {
            NetworkHelper.Log($"I AM a Host! {NetworkManager.ServerClientId}/{NetworkManager.LocalClientId}");
        }
        if (IsClient)
        {
            NetworkHelper.Log($"I AM a Client! {NetworkManager.LocalClientId}");
        }
        if (!IsServer && !IsClient)
        {
            NetworkHelper.Log("I AM Nothing yet");
        }
    }

    private void OnClientStarted()
    {
        NetworkHelper.Log("!! Client Started !!");
        NetworkManager.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.OnClientStopped += ClientOnClientStopped;
        PrintMe();
    }
    private void ClientOnClientStopped(bool indicator) 
    {
        NetworkHelper.Log("!! Client Stopped !!");
        NetworkManager.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.OnClientStopped -= ClientOnClientStopped;
        PrintMe();
    }

    private void ClientOnClientConnected(ulong clientID) {
        if (IsClient && !IsHost)
        {
            NetworkHelper.Log($"I have connected {clientID}");
        } else if (clientID == NetworkManager.ServerClientId)
        {
            NetworkHelper.Log($"I have connected {clientID}");
        }
    }
    private void ClientOnClientDisconnected(ulong clientID) {
        if (IsClient && !IsHost)
        {
            NetworkHelper.Log($"I have disconnected {clientID}");
        }
        else if (clientID == NetworkManager.ServerClientId)
        {
            NetworkHelper.Log($"I have disconnected {clientID}");
        }
    }

    private void OnServerStarted()
    {
        NetworkHelper.Log("!! Server Started !!");
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.OnServerStopped += ServerOnServerStopped;
        PrintMe();
    }
    private void ServerOnServerStopped(bool indicator)
    {
        NetworkHelper.Log("!! Server Stopped !!");
        NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.OnServerStopped -= ServerOnServerStopped;
        PrintMe();
    }

    private void ServerOnClientConnected(ulong clientID)
    {
        NetworkHelper.Log($"Client {clientID} connected to the server");
    }
    private void ServerOnClientDisconnected(ulong clientID)
    {
        NetworkHelper.Log($"Client {clientID} disconnected from the server");
    }
}
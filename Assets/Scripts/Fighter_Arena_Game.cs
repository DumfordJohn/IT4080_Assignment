using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Fighter_Arena_Game : NetworkBehaviour
{

    public Player playerPrefab;
    public Player hostPrefab;
    public Camera arenaCamera;
    public GameObject healthPickups;

    private NetworkedPlayers networkedPlayers;

    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3 (4, 2, 0),
        new Vector3 (-4, 2, 0),
        new Vector3 (0, 2, 4),
        new Vector3 (0, 2, -4)
    };

    private int hpPositionIndex = 0;
    private Vector3[] hpPositions = new Vector3[]
{
        new Vector3 (3, 2, 0),
        new Vector3 (-3, 2, 0),
        new Vector3 (0, 2, 3),
        new Vector3 (0, 2, -3)
};



    private int WrapInt(int curValue, int increment, int max)
    {
        int toReturn = curValue + increment;
        if(toReturn > max)
        {
            toReturn = 0;
        }
        return toReturn;
    }

    // Start is called before the first frame update
    void Start()
    {
        arenaCamera.enabled = !IsClient;
        arenaCamera.GetComponent<AudioListener>().enabled = !IsClient;

        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();

        if (IsServer)
        {
            SpawnPlayers();
            SpawnHealthPickUps();
        }
    }

    private Vector3 NextPosition()
    {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1)
        {
            positionIndex = 0;
        }
        return pos;
    }

    private Vector3 HPPickupNextPositions()
    {
        Vector3 pos = hpPositions[hpPositionIndex];
        hpPositionIndex += 1;
        if(hpPositionIndex > hpPositions.Length - 1)
        {
            hpPositionIndex = 0;
        }
        return pos;
    }

    private void SpawnPlayers()
    {
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            Player prefab = playerPrefab;
            Player playerSpawn = Instantiate(prefab, NextPosition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
            playerSpawn.PlayerColor.Value = info.color;
        }
    }

    private void SpawnHealthPickUps()
    {
        foreach(Vector3 hpSpawnLoc in hpPositions)
        {
            GameObject hpPickup = Instantiate(healthPickups, HPPickupNextPositions(), Quaternion.identity);
            hpPickup.GetComponent<NetworkObject>().Spawn();
        }
    }
}

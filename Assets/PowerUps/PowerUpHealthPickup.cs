using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PowerUpHealthPickup : NetworkBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<Player>())
        {
            this.GetComponent<NetworkObject>().Despawn();
        }
    }
}

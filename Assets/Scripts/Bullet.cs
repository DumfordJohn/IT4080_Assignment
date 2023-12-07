using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private float speed = 20f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq.Expressions;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    public NetworkVariable<int> ScoreNetVar = new NetworkVariable<int>(0);
    public NetworkVariable<int> playerHP = new NetworkVariable<int>();
    public BulletSpawner bulletSpawner;
    
    
    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    private Camera playerCamera;
    private GameObject playerBody;

    private void NetworkInit()
    {
        playerBody = transform.Find("Body").gameObject;
        playerCamera = transform.Find("Camera").GetComponent<Camera>();

        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        PlayerColor.OnValueChanged += OnPlayerColorChanged;
        ApplyPlayerColor();

        if (IsClient)
        {
            ScoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
    }

    private void Awake()
    {
        NetworkHelper.Log(this, "Awake");
    }

    void Start()
    {
        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn()
    {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
        playerHP.Value = 100;
    }

    private void ClientOnScoreValueChanged(int old, int current)
    {
        if (IsOwner)
        {
            NetworkHelper.Log(this, $"My score is {ScoreNetVar.Value}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            ServerHandleCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.CompareTag("power_up"))
            {
                other.GetComponent<BasePowerUp>().ServerPickUp(this);
            }
        }
        if (other.GetComponent<Bullet>())
        {
            NetworkManager.Singleton.ConnectedClients[other.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.GetComponent<NetwrokPlayerData>().score.Value += 1;
            playerHP.Value -= 10;
        }
        else if (other.GetComponent<PowerUpHealthPickup>())
        {
            playerHP.Value += 50;
        }
    }

    private void ServerHandleCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("bullet"))
        {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            NetworkHelper.Log(this, $"Hit by {collision.gameObject.name} " + $"owned by {ownerId}");
            //Player other = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            //other.ScoreNetVar.Value += 1;
            Destroy(collision.gameObject); 
            playerHP.Value -= 10;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            OwnerHandleMovementInput();
            if (Input.GetButtonDown("Fire1"))
            {
                NetworkHelper.Log("Requesting Fire");
                bulletSpawner.FireServerRpc();
            }
        }
    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }

    private void OwnerHandleMovementInput()
    {
        
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();
        if (movement != Vector3.zero || rotation != Vector3.zero)
        {
            MoveServerRpc(movement, rotation);
        }
    }

    private void ApplyPlayerColor()
    {
        NetworkHelper.Log(this, $"Applying color {PlayerColor.Value}");
        playerBody.GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 posChange, Vector3 rotChange)
    {
        transform.Translate(posChange);
        transform.Rotate(rotChange);
        
    }


    // Rotate around the y axis when shift is not pressed
    private Vector3 CalcRotation()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown)
        {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }


    // Move up and back, and strafe when shift is pressed
    private Vector3 CalcMovement()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }
}

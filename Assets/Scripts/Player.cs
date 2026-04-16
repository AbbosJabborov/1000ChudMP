using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // This is our player
            Debug.Log("My player spawned");
        }
        else
        {
            Debug.Log("Other player spawned");
        }
    }

    // Add movement or other logic here
}

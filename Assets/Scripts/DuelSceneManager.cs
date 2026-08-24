using Unity.Netcode;
using UnityEngine;

public class DuelSceneManager : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign in inspector

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            SpawnPlayersServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayersServerRpc()
    {
        foreach (var clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerObj = Instantiate(playerPrefab);
            playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}

using Unity.Netcode;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartHost()
    {
        Debug.Log("Starting as Host...");
        Unity.Netcode.NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Debug.Log("Starting as Client...");
        Unity.Netcode.NetworkManager.Singleton.StartClient();
    }

    public bool IsHost => Unity.Netcode.NetworkManager.Singleton.IsHost;
    public bool IsConnected => Unity.Netcode.NetworkManager.Singleton.IsServer || Unity.Netcode.NetworkManager.Singleton.IsClient;
    public ulong LocalClientId => Unity.Netcode.NetworkManager.Singleton.LocalClientId;
}

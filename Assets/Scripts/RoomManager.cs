using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkBehaviour
{
    private NetworkVariable<string> _roomCode = new NetworkVariable<string>();
    private NetworkVariable<bool> _isPublic = new NetworkVariable<bool>();
    private NetworkVariable<int> _playerCount = new NetworkVariable<int>();

    private bool _duelStarted = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            Debug.Log($"Room created: {_roomCode.Value}");
        }
        else
        {
            Debug.Log($"Joined room: {_roomCode.Value}");
        }

        // Subscribe to player join/leave events
        Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        UpdatePlayerCount();
    }

    public override void OnNetworkDespawn()
    {
        Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        base.OnNetworkDespawn();
    }

    public void SetRoomCode(string code, bool isPublicRoom)
    {
        if (IsHost)
        {
            _roomCode.Value = code;
            _isPublic.Value = isPublicRoom;
            _playerCount.Value = 1; // Host counts as 1 player
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        UpdatePlayerCount();

        // If we have 2 players and we're host, start duel
        if (IsHost && _playerCount.Value >= 2 && !_duelStarted)
        {
            _duelStarted = true;
            StartDuelGameClientRpc();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        if (IsHost)
        {
            _playerCount.Value = (int)Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds.Count;
            Debug.Log($"Players in room: {_playerCount.Value}");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void StartDuelGameClientRpc()
    {
        Debug.Log("All clients loading Duel scene...");
        SceneManager.LoadScene("DuelScene");
    }

    public string GetRoomCode() => _roomCode.Value;
    public bool GetIsPublic() => _isPublic.Value;
    public int GetPlayerCount() => _playerCount.Value;
}

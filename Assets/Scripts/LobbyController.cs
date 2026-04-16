using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button joinWithCodeButton;
    
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject joinPanel; // Panel with input field & join button
    
    [SerializeField] private Canvas lobbyCanvas;
    
    private string _currentRoomCode;

    private void Start()
    {
        // Hide join panel initially
        if (joinPanel != null)
            joinPanel.SetActive(false);

        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        joinWithCodeButton.onClick.AddListener(OnJoinWithCodeClicked);

        statusText.text = "Ready";
    }

    private void OnCreateRoomClicked()
    {
        statusText.text = "Creating room...";
        
        // Generate a simple room code (4-6 characters)
        _currentRoomCode = GenerateRoomCode();
        statusText.text = $"Room Code: {_currentRoomCode}\nWaiting for player...";

        // Start as Host
        NetworkManager.Instance.StartHost();
        
        // Spawn RoomManager on host
        if (NetworkManager.Instance.IsHost)
        {
            SpawnRoomManager(_currentRoomCode, isPublic: true);
        }

        // Disable buttons after creating
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    private void OnJoinRoomClicked()
    {
        // Show input panel for room code
        if (joinPanel != null)
            joinPanel.SetActive(true);
        
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    private void OnJoinWithCodeClicked()
    {
        string codeInput = roomCodeInputField.text.Trim().ToUpper();
        
        if (string.IsNullOrEmpty(codeInput))
        {
            statusText.text = "Please enter a room code";
            return;
        }

        statusText.text = $"Joining room {codeInput}...";
        _currentRoomCode = codeInput;

        // Start as Client
        NetworkManager.Instance.StartClient();

        // Hide panel
        if (joinPanel != null)
            joinPanel.SetActive(false);

        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    private void SpawnRoomManager(string roomCode, bool isPublic)
    {
        // Instantiate RoomManager as a networked object
        GameObject roomManagerObj = new GameObject("RoomManager");
        RoomManager roomManager = roomManagerObj.AddComponent<RoomManager>();
        roomManager.SetRoomCode(roomCode, isPublic);

        // Spawn it on the network
        NetworkObject netObj = roomManagerObj.AddComponent<NetworkObject>();
        netObj.Spawn();

        Debug.Log($"RoomManager spawned with code: {roomCode}");
    }

    private string GenerateRoomCode()
    {
        // Simple 4-character alphanumeric code
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";
        for (int i = 0; i < 4; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }
        return code;
    }

    public void SetStatus(string message)
    {
        statusText.text = message;
    }
}

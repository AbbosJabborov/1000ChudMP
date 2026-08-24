using Unity.Netcode;
using UnityEngine;

public class DuelController : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private int charactersPerDuel = 12; // 6 pairs
    
    private NetworkVariable<int> _player1Score = new NetworkVariable<int>(0);
    private NetworkVariable<int> _player2Score = new NetworkVariable<int>(0);
    
    private NetworkVariable<ulong> _currentPlayerTurn = new NetworkVariable<ulong>();
    private NetworkVariable<float> _turnStartTime = new NetworkVariable<float>(0);
    private NetworkVariable<int> _roundNumber = new NetworkVariable<int>(1);
    
    private float _baseTurnTime = 30f; // Base time limit
    private float _timeIncrement = 2f; // Increases by 2 seconds each round
    
    private Character[] _gameCharacters;
    private Character _selectedChar1;
    private Character _selectedChar2;
    
    public static DuelController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            // Initialize the duel
            InitializeDuel();
        }
    }

    private void InitializeDuel()
    {
        // Get random character set
        _gameCharacters = characterDatabase.GetRandomCharacterSet(charactersPerDuel);
        
        // Host is Player 1, starts first
        _currentPlayerTurn.Value = Unity.Netcode.NetworkManager.ServerClientId;
        _turnStartTime.Value = Time.time;
        _roundNumber.Value = 1;
        
        _player1Score.Value = 0;
        _player2Score.Value = 0;

        Debug.Log($"Duel initialized. Host is Player 1 with Server ID: {Unity.Netcode.NetworkManager.ServerClientId}");
    }

    private void Update()
    {
        if (!IsSpawned) return;

        // Check if turn time has expired
        if (IsHost)
        {
            float timeElapsed = Time.time - _turnStartTime.Value;
            float currentTurnTimeLimit = GetCurrentTurnTimeLimit();

            if (timeElapsed > currentTurnTimeLimit)
            {
                // Time's up, switch turns
                OnTurnTimeExpiredServerRpc();
            }
        }
    }

    /// <summary>
    /// Player selects first character
    /// </summary>
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex >= _gameCharacters.Length)
            return;

        // Only allow selection on current player's turn
        if (!IsCurrentPlayer())
        {
            Debug.LogWarning("Not your turn!");
            return;
        }

        if (_selectedChar1 == null)
        {
            _selectedChar1 = _gameCharacters[characterIndex];
            Debug.Log($"Selected character 1: {_selectedChar1.name}");
        }
        else if (_selectedChar2 == null)
        {
            _selectedChar2 = _gameCharacters[characterIndex];
            Debug.Log($"Selected character 2: {_selectedChar2.name}");

            // Submit the match
            SubmitMatchServerRpc(_selectedChar1.id, _selectedChar2.id);
        }
    }

    [Rpc(SendTo.Server)]
    private void SubmitMatchServerRpc(int char1Id, int char2Id)
    {
        float timeUsed = Time.time - _turnStartTime.Value;
        
        Character char1 = GetCharacterById(char1Id);
        Character char2 = GetCharacterById(char2Id);

        bool isCorrectMatch = characterDatabase.AreCharactersMatched(char1, char2);
        int pointsEarned = CalculatePoints(isCorrectMatch, timeUsed);

        if (isCorrectMatch)
        {
            Debug.Log($"Correct match! {char1.name} and {char2.name} share category: {char1.category}");
        }
        else
        {
            Debug.Log($"Wrong match! {char1.name} ({char1.category}) and {char2.name} ({char2.category})");
        }

        // Award points to current player
        if (_currentPlayerTurn.Value == Unity.Netcode.NetworkManager.ServerClientId)
        {
            _player1Score.Value += pointsEarned;
        }
        else
        {
            _player2Score.Value += pointsEarned;
        }

        // Reset selection
        _selectedChar1 = null;
        _selectedChar2 = null;

        // Switch turns or end duel
        if (_roundNumber.Value < 3) // 3 rounds per player
        {
            _roundNumber.Value++;
            SwitchTurn();
        }
        else
        {
            EndDuel();
        }
    }

    [Rpc(SendTo.Server)]
    private void OnTurnTimeExpiredServerRpc()
    {
        Debug.Log("Turn time expired!");
        _selectedChar1 = null;
        _selectedChar2 = null;

        SwitchTurn();
    }

    private void SwitchTurn()
    {
        // Get connected clients
        var connectedClients = Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds;
        
        if (connectedClients.Count < 2)
        {
            Debug.LogError("Not enough players!");
            return;
        }

        // Toggle between the two players
        if (_currentPlayerTurn.Value == connectedClients[0])
        {
            _currentPlayerTurn.Value = connectedClients[1];
        }
        else
        {
            _currentPlayerTurn.Value = connectedClients[0];
        }

        _turnStartTime.Value = Time.time;
        Debug.Log($"Turn switched to player: {_currentPlayerTurn.Value}");
    }

    private void EndDuel()
    {
        Debug.Log($"Duel ended! Player 1: {_player1Score.Value}, Player 2: {_player2Score.Value}");
        // Load results screen or return to lobby
    }

    private int CalculatePoints(bool isCorrect, float timeUsed)
    {
        if (!isCorrect) return 0;

        float timeLimit = GetCurrentTurnTimeLimit();
        float timeRemaining = timeLimit - timeUsed;
        
        // Points based on speed: faster = more points
        int basePoints = 100;
        int bonusPoints = Mathf.Max(0, Mathf.RoundToInt(timeRemaining * 10));
        
        return basePoints + bonusPoints;
    }

    private Character GetCharacterById(int id)
    {
        foreach (Character c in _gameCharacters)
        {
            if (c.id == id) return c;
        }
        return null;
    }

    private bool IsCurrentPlayer()
    {
        return _currentPlayerTurn.Value == Unity.Netcode.NetworkManager.Singleton.LocalClientId;
    }

    // Getters for UI
    public Character[] GetGameCharacters() => _gameCharacters;
    public int GetPlayer1Score() => _player1Score.Value;
    public int GetPlayer2Score() => _player2Score.Value;
    public float GetCurrentTurnTimeLimit() => _baseTurnTime + (_roundNumber.Value - 1) * _timeIncrement;
    public float GetRemainingTurnTime() => GetCurrentTurnTimeLimit() - (Time.time - _turnStartTime.Value);
    public bool IsCurrentPlayersTurn() => IsCurrentPlayer();
    public int GetCurrentRound() => _roundNumber.Value;
    public ulong GetCurrentPlayerTurnId() => _currentPlayerTurn.Value;
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DuelUIManager : MonoBehaviour
{
    [SerializeField] private Transform cardGridParent; // Parent transform for card buttons
    [SerializeField] private GameObject cardButtonPrefab; // Button prefab for each character
    
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private TextMeshProUGUI roundText;
    
    [SerializeField] private Image player1HighlightImage; // Highlight when player 1's turn
    [SerializeField] private Image player2HighlightImage; // Highlight when player 2's turn
    
    private Button[] _cardButtons;
    private Character[] _gameCharacters;
    private DuelController _duelController;

    private void Start()
    {
        _duelController = DuelController.Instance;
        
        if (_duelController == null)
        {
            Debug.LogError("DuelController not found!");
            return;
        }

        _gameCharacters = _duelController.GetGameCharacters();
        InitializeCardGrid();
    }

    private void InitializeCardGrid()
    {
        // Clear existing children
        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }

        _cardButtons = new Button[_gameCharacters.Length];

        for (int i = 0; i < _gameCharacters.Length; i++)
        {
            int index = i; // Local copy for closure
            
            GameObject cardObj = Instantiate(cardButtonPrefab, cardGridParent);
            Button cardButton = cardObj.GetComponent<Button>();
            
            // Set up button visuals
            Image cardImage = cardObj.GetComponent<Image>();
            if (cardImage != null && _gameCharacters[i].image != null)
            {
                cardImage.sprite = _gameCharacters[i].image;
            }
            
            // Set button text with character name
            TextMeshProUGUI cardText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
            if (cardText != null)
            {
                cardText.text = _gameCharacters[i].name;
            }

            // Wire click event
            cardButton.onClick.AddListener(() => OnCardClicked(index));
            _cardButtons[i] = cardButton;
        }
    }

    private void OnCardClicked(int index)
    {
        if (!_duelController.IsCurrentPlayersTurn())
        {
            Debug.LogWarning("It's not your turn!");
            return;
        }

        _duelController.SelectCharacter(index);
        
        // Visual feedback: highlight selected card
        _cardButtons[index].image.color = Color.yellow;
        
        // Reset color after a short delay (optional)
        Invoke(nameof(ResetCardColors), 0.5f);
    }

    private void ResetCardColors()
    {
        foreach (Button btn in _cardButtons)
        {
            btn.image.color = Color.white;
        }
    }

    private void Update()
    {
        UpdateTimer();
        UpdateScores();
        UpdateTurnIndicator();
        UpdateRound();
    }

    private void UpdateTimer()
    {
        float remainingTime = _duelController.GetRemainingTurnTime();
        remainingTime = Mathf.Max(0, remainingTime);
        
        timerText.text = $"Time: {remainingTime:F1}s";
        
        // Warning color if time is low
        if (remainingTime < 5f)
        {
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    private void UpdateScores()
    {
        player1ScoreText.text = $"Player 1: {_duelController.GetPlayer1Score()}";
        player2ScoreText.text = $"Player 2: {_duelController.GetPlayer2Score()}";
    }

    private void UpdateTurnIndicator()
    {
        ulong currentPlayerTurnId = _duelController.GetCurrentPlayerTurnId();
        var connectedClients = Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds;
        
        if (connectedClients.Count < 2) return;

        bool isPlayer1Turn = currentPlayerTurnId == connectedClients[0];
        
        if (isPlayer1Turn)
        {
            turnIndicatorText.text = "Player 1's Turn";
            player1HighlightImage.color = new Color(0, 1, 0, 0.3f); // Green highlight
            player2HighlightImage.color = Color.clear;
        }
        else
        {
            turnIndicatorText.text = "Player 2's Turn";
            player2HighlightImage.color = new Color(0, 1, 0, 0.3f); // Green highlight
            player1HighlightImage.color = Color.clear;
        }
    }

    private void UpdateRound()
    {
        roundText.text = $"Round: {_duelController.GetCurrentRound()} / 3";
    }
}

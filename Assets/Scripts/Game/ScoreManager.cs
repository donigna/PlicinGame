using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public PlinkoManager plinkoManager;
    public LeaderboardAPI leaderboardAPI;
    public TextMeshProUGUI ballCountTXT;
    public TextMeshProUGUI roundMoneyTXT;
    public TextMeshProUGUI dropCostTXT;
    public Button dropButton;

    [SerializeField] string username = "Player";
    [SerializeField] int ballCount = 1;
    [SerializeField] float initialMoney = 100;
    [SerializeField] float currentMoney;
    [SerializeField] float ballCost = 5;
    float highestMoney = 0;

    public void SetUsername(string newUsername)
    {
        if (!string.IsNullOrEmpty(newUsername))
        {
            username = newUsername.Trim();
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();
            Debug.Log($"Username set to: {username}");
        }
    }

    public string GetUsername()
    {
        return username;
    }

    public void LoadPlayerData(LeaderboardEntry playerData)
    {
        if (playerData != null)
        {
            // Load player's highest money from API
            highestMoney = playerData.uang;
            Debug.Log($"Loaded player data - Highest Money: {highestMoney}");
            
            // You can load other data here if needed
            // For example: level, gameCount, etc.
        }
        else
        {
            // New player, use initial money
            highestMoney = initialMoney;
            Debug.Log("New player detected, using initial money");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UpdateAllUI();
        dropButton.onClick.AddListener(() => DropBall());

        currentMoney = initialMoney;
        
        // Load saved username if available
        if (PlayerPrefs.HasKey("Username"))
        {
            username = PlayerPrefs.GetString("Username");
        }
        else
        {
            highestMoney = initialMoney;
        }
    }

    void Update()
    {
        dropButton.interactable = (currentMoney >= ballCost * ballCount) && !plinkoManager.isDroping();
    }

    private void UpdateAllUI()
    {
        ballCountTXT.text = ballCount.ToString();
        roundMoneyTXT.text = initialMoney.ToString();
        dropCostTXT.text = (ballCost * ballCount).ToString();
    }

    private void DropBall()
    {
        plinkoManager.DropBall(ballCount);
        currentMoney -= ballCost * ballCount;
        roundMoneyTXT.text = currentMoney.ToString();
    }

    public void IncreaseBall()
    {
        ballCount++;
        UpdateBallCountUI();
    }

    public void DecreaseBall()
    {
        if (ballCount <= 1) return;

        ballCount--;
        UpdateBallCountUI();
    }

    public void UpdateBallCountUI()
    {
        ballCountTXT.text = ballCount.ToString();
        dropCostTXT.text = (ballCost * ballCount).ToString();
    }

    public void SubmitScore()
    {
        if (leaderboardAPI != null)
        {
            // If username is null or empty, submit a new score with default name
            if (string.IsNullOrEmpty(username))
            {
                leaderboardAPI.SubmitNewScore("Player", highestMoney);
            }
            else
            {
                // Check if player exists, then update or submit accordingly
                StartCoroutine(CheckAndSubmitScore());
            }
        }
    }

    private System.Collections.IEnumerator CheckAndSubmitScore()
    {
        bool playerExists = false;
        int playerId = -1;

        // Check if player exists
        yield return leaderboardAPI.CheckPlayerExists(username, (exists, id) =>
        {
            playerExists = exists;
            playerId = id;
        });

        // Update existing player or submit new score
        if (playerExists && playerId >= 0)
        {
            leaderboardAPI.UpdateScore(username, highestMoney, playerId);
        }
        else
        {
            leaderboardAPI.SubmitNewScore(username, highestMoney);
        }
    }

    public void AddByMultiplier(float multiplier, bool isLastBall = false)
    {
        currentMoney += ballCost * multiplier;
        UpdateScoreUI();

        if (currentMoney > highestMoney)
        {
            highestMoney = currentMoney;
            if (isLastBall)
            {
                SubmitScore();
            }
        }

    }

    private System.Collections.IEnumerator SubmitScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SubmitScore();
    }

    private void UpdateScoreUI()
    {
        if (roundMoneyTXT != null)
        {
            roundMoneyTXT.text = currentMoney.ToString("0.0");
        }
    }
}

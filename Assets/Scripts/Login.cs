using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public TMP_InputField _inputField;
    public Button loginBtn;
    public LeaderboardAPI leaderboardAPI;
    [SerializeField] private string gameSceneName = "MainScene"; // <- atur nama scene target di Inspector

    private void Awake()
    {
        // Load saved username if available
        if (PlayerPrefs.HasKey("Username"))
        {
            string savedUsername = PlayerPrefs.GetString("Username");
            if (_inputField != null)
            {
                _inputField.text = savedUsername;
            }
        }

        if (loginBtn != null)
        {
            loginBtn.onClick.AddListener(OnLoginClicked);
        }

        if (_inputField != null)
        {
            _inputField.onSubmit.AddListener((value) => OnLoginClicked());
        }
    }

    private void Start()
    {
        if (_inputField != null)
        {
            _inputField.Select();
            _inputField.ActivateInputField();
        }

        if (leaderboardAPI == null)
        {
            leaderboardAPI = FindAnyObjectByType<LeaderboardAPI>();
        }
    }

    public void OnLoginClicked()
    {
        if (_inputField == null)
        {
            Debug.LogError("Input field is not assigned!");
            return;
        }

        string inputUsername = _inputField.text;

        if (string.IsNullOrWhiteSpace(inputUsername))
        {
            Debug.LogWarning("Please enter a username!");
            return;
        }

        inputUsername = inputUsername.Trim();

        if (loginBtn != null)
        {
            loginBtn.interactable = false;
        }

        if (leaderboardAPI != null)
        {
            leaderboardAPI.GetPlayerDataByUsername(inputUsername, (success, playerData) =>
            {
                if (success && playerData != null)
                {
                    OnLoginSuccess(inputUsername, playerData);
                }
                else
                {
                    OnLoginSuccess(inputUsername, null);
                }
            });
        }
        else
        {
            OnLoginSuccess(inputUsername, null);
        }
    }

    private void OnLoginSuccess(string username, LeaderboardEntry playerData)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetUsername(username);

            if (playerData != null)
            {
                ScoreManager.Instance.LoadPlayerData(playerData);
                Debug.Log($"Login successful! Loaded data for: {username}");
            }
            else
            {
                Debug.Log($"Login successful! New player: {username}");
            }
        }
        else
        {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();
            Debug.Log($"Username saved: {username}");
        }

        if (loginBtn != null)
        {
            loginBtn.interactable = true;
        }

        // 👉 Pindah ke scene game
        SceneManager.LoadScene(gameSceneName);
    }
}

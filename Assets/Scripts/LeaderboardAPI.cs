using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardEntry
{
    public int id;
    public string playerName;
    public int uang;
    public int level;
    public int gameCount;
    public string createdAt;
    public string updatedAt;
}

public class LeaderboardAPI : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5115/api/Player";
    [SerializeField] private Transform leaderboardParent;
    [SerializeField] private TMP_Text entryPrefab;

    void Start()
    {
        StartCoroutine(GetLeaderboard());
    }

    public IEnumerator GetLeaderboard()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching leaderboard: " + request.error);
            }
            else
            {
                string json = request.downloadHandler.text;
                LeaderboardEntry[] entries = JsonConvert.DeserializeObject<LeaderboardEntry[]>(json);

                DisplayLeaderboard(entries);
            }
        }
    }

    public IEnumerator SubmitScore(string playerName, int playerScore)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            playerName = playerName,
            uang = playerScore,
            gameCount = 1
        };

        string json = JsonUtility.ToJson(entry);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Submit failed: " + request.error);
            }
            else
            {
                Debug.Log("Score submitted successfully!");
                StartCoroutine(GetLeaderboard()); // refresh
            }
        }
    }

    void DisplayLeaderboard(LeaderboardEntry[] entries)
    {
        if (entries == null || entries.Length == 0)
        {
            Debug.LogWarning("Leaderboard kosong atau gagal diparsing.");
            return;
        }

        if (leaderboardParent == null || entryPrefab == null)
        {
            Debug.LogError("Parent/prefab belum diassign di Inspector!");
            return;
        }

        foreach (Transform child in leaderboardParent)
            Destroy(child.gameObject);

        foreach (var e in entries)
        {
            TMP_Text text = Instantiate(entryPrefab, leaderboardParent);
            text.text = $"{e.playerName} - {e.uang}";
        }
    }
}

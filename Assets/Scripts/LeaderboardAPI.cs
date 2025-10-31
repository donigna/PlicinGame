using System;
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
    [SerializeField] private string baseUrl = "http://localhost:5115/api/Player/";
    [SerializeField] private Transform leaderboardParent;
    [SerializeField] private TMP_Text entryPrefab;
    [SerializeField] private TMP_InputField namaTxt;
    [SerializeField] private TMP_InputField scoreTxt;

    void Start()
    {
        StartCoroutine(GetLeaderboard());
        StartCoroutine(AutoRefreshLoop());
    }

    public void RefreshLeaderboard()
    {
        StartCoroutine(GetLeaderboard());
    }

    public IEnumerator AutoRefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // refresh tiap 10 detik
            yield return GetLeaderboard();
        }
    }

    public IEnumerator GetLeaderboard()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/top"))
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

    public void SubmitNewScore()
    {
        if (int.TryParse(scoreTxt.text, out int score))
        {
            StartCoroutine(SubmitScore(namaTxt.text, score));
        }
        else
        {
            Debug.LogWarning($"Input skor tidak valid: '{scoreTxt.text}'");
        }
    }

    public IEnumerator SubmitScore(string playerName, int playerScore)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            id = 0,
            playerName = playerName,
            uang = playerScore,
            level = 1,
            gameCount = 1,
            createdAt = DateTime.UtcNow.ToString("o"),
            updatedAt = DateTime.UtcNow.ToString("o")
        };

        string json = JsonConvert.SerializeObject(entry,
        new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

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

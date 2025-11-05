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
    public float uang;
    public int level;
    public int gameCount;
    public string createdAt;
    public string updatedAt;
}

public class LeaderboardAPI : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5115/api/Player/";
    [SerializeField] private Transform leaderboardParent;
    [SerializeField] private LeaderboardText entryPrefab;

    void Start()
    {
        if (leaderboardParent != null)
        {
            StartCoroutine(GetLeaderboard());
            StartCoroutine(AutoRefreshLoop());
        }
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
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "top"))
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

    public void SubmitNewScore(string playerName, float playerScore)
    {
        StartCoroutine(SubmitScore(playerName, playerScore));
    }

    public IEnumerator SubmitScore(string playerName, float playerScore)
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

    public IEnumerator CheckPlayerExists(string playerName, System.Action<bool, int> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error checking player: " + request.error);
                callback?.Invoke(false, -1);
            }
            else
            {
                string json = request.downloadHandler.text;
                LeaderboardEntry[] entries = JsonConvert.DeserializeObject<LeaderboardEntry[]>(json);

                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.playerName == playerName)
                        {
                            callback?.Invoke(true, entry.id);
                            yield break;
                        }
                    }
                }
                callback?.Invoke(false, -1);
            }
        }
    }

    public void GetPlayerDataByUsername(string playerName, System.Action<bool, LeaderboardEntry> callback)
    {
        StartCoroutine(GetPlayerDataCoroutine(playerName, callback));
    }

    public IEnumerator GetPlayerDataCoroutine(string playerName, System.Action<bool, LeaderboardEntry> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching player data: " + request.error);
                callback?.Invoke(false, null);
            }
            else
            {
                string json = request.downloadHandler.text;
                LeaderboardEntry[] entries = JsonConvert.DeserializeObject<LeaderboardEntry[]>(json);

                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.playerName == playerName)
                        {
                            callback?.Invoke(true, entry);
                            yield break;
                        }
                    }
                }
                // Player not found
                callback?.Invoke(false, null);
            }
        }
    }

    public void UpdateScore(string playerName, float playerScore, int playerId)
    {
        StartCoroutine(UpdateScoreCoroutine(playerName, playerScore, playerId));
    }

    public IEnumerator UpdateScoreCoroutine(string playerName, float playerScore, int playerId)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            id = playerId,
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

        // Try PUT first
        string url = baseUrl + playerId;
        Debug.Log($"Attempting to update score at: {url}");

        using (UnityWebRequest request = UnityWebRequest.Put(url, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"PUT failed ({request.responseCode}): {request.error}. Trying POST with ID...");

                // // If PUT fails, try POST with ID (some APIs handle updates via POST)
                // yield return UpdateScoreViaPost(playerName, playerScore, playerId, json);
            }
            else
            {
                Debug.Log("Score updated successfully via PUT!");
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
            LeaderboardText entry = Instantiate(entryPrefab, leaderboardParent);
            entry.username.text = e.playerName;
            entry.highscore.text = e.uang.ToString();
        }
    }
}

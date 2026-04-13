using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    private bool isLoggedIn = false;
    private string playFabId;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Login();
    }

    // ================= LOGIN =================
    void Login()
    {
        // 🔥 FIX: KHÔNG dùng Guid mỗi lần → dùng PlayerPrefs
        string customId = PlayerPrefs.GetString("PF_ID", "");

        if (string.IsNullOrEmpty(customId))
        {
            customId = SystemInfo.deviceUniqueIdentifier + "_" + Random.Range(0, 9999);
            PlayerPrefs.SetString("PF_ID", customId);
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                Debug.Log("✅ LOGIN SUCCESS");

                isLoggedIn = true;
                playFabId = result.PlayFabId;

                // 🔥 lấy tên đã nhập
                string savedName = PlayerPrefs.GetString("PlayerName", "NoName");
                SetPlayerName(savedName);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
    }

    // ================= SET NAME =================
    public void SetPlayerName(string playerName)
    {
        if (!isLoggedIn) return;

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => Debug.Log("🔥 NAME SET: " + playerName),
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // ================= SUBMIT =================
    public void SubmitScore(int score, int time)
    {
        StartCoroutine(SubmitWhenReady(score, time));
    }

    IEnumerator SubmitWhenReady(int score, int time)
    {
        while (!isLoggedIn)
            yield return null;

        // 🔥 công thức rank
        int rankScore = score * 100000 + (10000 - time);

        var stats = new List<StatisticUpdate>()
        {
            new StatisticUpdate
            {
                StatisticName = "RankScore",
                Value = rankScore
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest { Statistics = stats },
            result =>
            {
                Debug.Log($"🔥 SUBMIT SUCCESS | Score={score} Time={time}");
                GetLeaderboard();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // ================= LEADERBOARD =================
    public void GetLeaderboard()
    {
        if (!isLoggedIn) return;

        var request = new GetLeaderboardRequest
        {
            StatisticName = "RankScore",
            StartPosition = 0,
            MaxResultsCount = 20
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result =>
            {
                Debug.Log("🔥 LOAD LEADERBOARD");

                LeaderboardUI ui = Object.FindFirstObjectByType<LeaderboardUI>();

                if (ui != null)
                    ui.ShowLeaderboard(result.Leaderboard, playFabId);
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}
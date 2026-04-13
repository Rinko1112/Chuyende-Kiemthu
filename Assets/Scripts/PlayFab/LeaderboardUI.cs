using UnityEngine;
using System.Collections.Generic;
using PlayFab.ClientModels;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public GameObject itemPrefab;

    private List<GameObject> items = new List<GameObject>();

    public void Open()
    {
        panel.SetActive(true);
        PlayFabManager.Instance.GetLeaderboard();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void ShowLeaderboard(List<PlayerLeaderboardEntry> leaderboard, string myId)
    {
        Clear();

        foreach (var player in leaderboard)
        {
            GameObject obj = Instantiate(itemPrefab, content);

            var ui = obj.GetComponent<LeaderboardItemUI>();

            string name = string.IsNullOrEmpty(player.DisplayName)
                ? "NoName"
                : player.DisplayName;

            int encoded = player.StatValue;

            int score = encoded / 100000;
            int time = 10000 - (encoded % 100000);

            bool isMe = player.PlayFabId == myId;

            ui.Setup(player.Position + 1, name, score, time, isMe);

            items.Add(obj);
        }
    }

    void Clear()
    {
        foreach (var item in items)
            Destroy(item);

        items.Clear();
    }
}
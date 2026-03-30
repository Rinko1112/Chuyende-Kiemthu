using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerListManager : MonoBehaviour
{
    public static PlayerListManager Instance;

    // ===== DATA =====
    private Dictionary<int, (string name, bool isDead)> players 
        = new Dictionary<int, (string, bool)>();

    private string lastData = "";

    // ===== UI =====
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject textPrefab;

    [Header("OPEN BUTTON")]
    [SerializeField] private GameObject openButton;

    [Header("NOTIFY")]
    [SerializeField] private GameObject notifyDot;

    // ===== ANIMATION =====
    [Header("ANIMATION")]
    [SerializeField] private float animDuration = 0.2f;

    private bool isOpening = false;
    private float animTime = 0f;

    private List<GameObject> items = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);

        if (openButton != null)
            openButton.SetActive(true);

        if (notifyDot != null)
            notifyDot.SetActive(false);
    }

    void Update()
    {
        // ===== ANIMATION =====
        if (isOpening && panel != null)
        {
            animTime += Time.deltaTime;
            float t = animTime / animDuration;

            panel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            if (t >= 1f)
            {
                panel.transform.localScale = Vector3.one;
                isOpening = false;
            }
        }

        // ===== REFRESH LIST =====
        if (panel != null && panel.activeSelf)
        {
            RefreshList();
        }
    }

    // ===== PLAYER LOGIC =====

    public void AddPlayer(int playerId, string name)
    {
        if (!players.ContainsKey(playerId))
        {
            players[playerId] = (name, false);
        }
        else
        {
            players[playerId] = (name, players[playerId].isDead);
        }
    }

    public void SetDead(int playerId, bool isDead)
    {
        if (players.ContainsKey(playerId))
        {
            var data = players[playerId];
            players[playerId] = (data.name, isDead);
        }
    }

    public void RemovePlayer(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
        }
    }

    public List<string> GetPlayerList()
{
    List<string> list = new List<string>();

    foreach (var p in players)
    {
        string name = p.Value.name;

        // 🔥 HOST
        if (p.Key == 1)
            name += " [Host]";

        if (p.Value.isDead)
            name += " [dead]";

        name += $"_player{p.Key}";

        list.Add(name);
    }

    return list;
}

    public void OpenPanel()
    {
        if (panel == null) return;

        panel.SetActive(true);

        if (openButton != null)
            openButton.SetActive(false);

        // 🔥 reset animation
        panel.transform.localScale = Vector3.zero;
        animTime = 0f;
        isOpening = true;

        // 🔥 tắt notify khi mở
        if (notifyDot != null)
            notifyDot.SetActive(false);

        RefreshList();
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        if (openButton != null)
            openButton.SetActive(true);
    }

    public void TogglePanel()
    {
        if (panel == null) return;

        if (panel.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

void RefreshList()
{
    foreach (var obj in items)
    {
        Destroy(obj);
    }
    items.Clear();

    List<string> list = GetPlayerList();

    foreach (string playerName in list)
    {
        GameObject item = Instantiate(textPrefab, content);
        TextMeshProUGUI txt = item.GetComponent<TextMeshProUGUI>();

        txt.text = playerName;

        items.Add(item);
    }

    // 🔥 FIX LAYOUT
    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
}
    public string GetRawData()
    {
        List<string> list = new List<string>();

        foreach (var p in players)
        {
            list.Add($"{p.Key}|{p.Value.name}|{(p.Value.isDead ? 1 : 0)}");
        }

        return string.Join(";", list);
    }

    public void SetFullData(string data)
    {
        // 🔥 detect thay đổi (player join / update)
        if (!string.IsNullOrEmpty(lastData) && lastData != data)
        {
            if (panel != null && !panel.activeSelf)
            {
                if (notifyDot != null)
                    notifyDot.SetActive(true);
            }
        }

        lastData = data;

        players.Clear();

        if (string.IsNullOrEmpty(data)) return;

        string[] entries = data.Split(';');

        foreach (var e in entries)
        {
            string[] parts = e.Split('|');

            if (parts.Length != 3) continue;

            int id = int.Parse(parts[0]);
            string name = parts[1];
            bool dead = parts[2] == "1";

            players[id] = (name, dead);
        }
    }
}
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    [Header("UI")]
    public GameObject chatPanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI chatContent;
    public ScrollRect scrollRect;
    private bool isSpawned = false; 

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.2f;

    [Header("Notify")]
    [SerializeField] private GameObject notifyDot;

    [Header("Open Button")]
    [SerializeField] private GameObject openChatButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sendSound;
    [SerializeField] private AudioClip receiveSound;

    private PlayerController localPlayer;

    private bool isOpening = false;
    private float animTime = 0f;

    private Dictionary<string, string> playerColors = new Dictionary<string, string>();

    private string[] colorList = new string[]
    {
        "#FF6B6B",
        "#4ECDC4",
        "#FFD93D",
        "#6C5CE7",
        "#00FFAB",
        "#FF9F1C"
    };

    // ================= 🔥 CHAT HISTORY (NEW) =================
    [Networked, Capacity(50)]
    private NetworkArray<NetworkString<_128>> ChatHistory => default;

    private string lastHistorySnapshot = "";

    // =======================================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        chatPanel.SetActive(false);

        if (notifyDot != null)
            notifyDot.SetActive(false);
    }

    [System.Obsolete]
    void Update()
{
    if (!isSpawned) return; // 🔥 FIX QUAN TRỌNG

    if (localPlayer == null)
        localPlayer = PlayerController.Local;

    // ===== ANIMATION =====
    if (isOpening)
    {
        animTime += Time.deltaTime;
        float t = animTime / animDuration;

        chatPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

        if (t >= 1f)
        {
            chatPanel.transform.localScale = Vector3.one;
            isOpening = false;
        }
    }

    // ===== ENTER =====
    if (Input.GetKeyDown(KeyCode.Return))
    {
        if (!chatPanel.activeSelf)
        {
            OpenChat();
            return;
        }

        if (inputField.isFocused)
        {
            OnSendButton();
        }
        else
        {
            inputField.ActivateInputField();
        }
    }

    // ===== ESC =====
    if (chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
        CloseChat();
    }

    // 🔥 SAFE CALL
    UpdateChatFromHistory();
}
    public void OnSendButton()
{
    if (localPlayer == null) return;

    string msg = inputField.text.Trim();
    if (string.IsNullOrEmpty(msg)) return;

    string playerName = localPlayer.GetPlayerName();

    RPC_SendChatToState(playerName, msg);

    if (audioSource != null && sendSound != null)
        audioSource.PlayOneShot(sendSound);

    inputField.text = "";
    inputField.ActivateInputField();
}

    // ================= 🔥 STORE HISTORY =================
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SendChatToState(string playerName, string message)
    {
        string full = playerName + "|" + message;

        for (int i = 0; i < ChatHistory.Length; i++)
        {
            if (string.IsNullOrEmpty(ChatHistory[i].ToString()))
            {
                ChatHistory.Set(i, full);
                return;
            }
        }

        // 🔥 nếu đầy → shift lên (giữ 50 tin mới nhất)
        for (int i = 1; i < ChatHistory.Length; i++)
        {
            ChatHistory.Set(i - 1, ChatHistory[i]);
        }

        ChatHistory.Set(ChatHistory.Length - 1, full);
    }

    // ================= 🔥 LOAD HISTORY =================
    void UpdateChatFromHistory()
    {
        string snapshot = "";

        for (int i = 0; i < ChatHistory.Length; i++)
        {
            string entry = ChatHistory[i].ToString();
            if (string.IsNullOrEmpty(entry)) continue;

            snapshot += entry + "\n";
        }

        if (snapshot == lastHistorySnapshot) return;

        lastHistorySnapshot = snapshot;

        chatContent.text = "";

        foreach (var line in snapshot.Split('\n'))
        {
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split('|');
            if (parts.Length < 2) continue;

            ReceiveMessage(parts[0], parts[1]);
        }
    }

    // ================= COLOR =================
    string GetColor(string playerName)
    {
        if (!playerColors.ContainsKey(playerName))
        {
            int index = playerColors.Count % colorList.Length;
            playerColors[playerName] = colorList[index];
        }

        return playerColors[playerName];
    }

    // ================= RECEIVE =================
    public void ReceiveMessage(string playerName, string message)
    {
        string color = GetColor(playerName);

        string fullMsg = $"<color={color}>{playerName}</color>: {message}";
        chatContent.text += fullMsg + "\n";

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;

        if (!chatPanel.activeSelf && notifyDot != null)
            notifyDot.SetActive(true);

        if (audioSource != null && receiveSound != null)
            audioSource.PlayOneShot(receiveSound);
    }

    // ================= OPEN =================
    public void OpenChat()
    {
        chatPanel.SetActive(true);

        if (openChatButton != null)
            openChatButton.SetActive(false);

        chatPanel.transform.localScale = Vector3.zero;
        animTime = 0f;
        isOpening = true;

        inputField.text = "";
        inputField.ActivateInputField();

        if (notifyDot != null)
            notifyDot.SetActive(false);

        if (PlayerController.Local != null)
            PlayerController.Local.SetControl(false);
    }

    // ================= CLOSE =================
    public void CloseChat()
    {
        chatPanel.SetActive(false);

        if (openChatButton != null)
            openChatButton.SetActive(true);

        if (PlayerController.Local != null)
            PlayerController.Local.SetControl(true);
    }
    public override void Spawned()
{
    isSpawned = true;
}
}
using UnityEngine;
using TMPro;

public class NameInputUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject panel;

    [Header("Status UI")]
    public TextMeshProUGUI statusText;

    private PlayerController localPlayer;
    [Header("AUDIO")]
[SerializeField] private AudioSource bgmSource;
[SerializeField] private AudioClip bgmClip;

    void Start()
    {
        panel.SetActive(true);

        if (statusText != null)
            statusText.text = "Đang kết nối server...";
    }

    void Update()
    {
        // 🔥 tìm player (FIX API Unity 6)
        if (localPlayer == null)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

            foreach (var p in players)
            {
                if (p.Object != null && p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    localPlayer.SetControl(false);

                    Debug.Log("FOUND LOCAL PLAYER");

                    if (statusText != null)
                        statusText.text = "Đã kết nối! Nhập tên của bạn";

                    break;
                }
            }
        }
    }

    public void OnConfirm()
{
    if (localPlayer == null)
    {
        if (statusText != null)
            statusText.text = "Vui lòng đợi kết nối server...";
        return;
    }

    string name = inputField.text;

    if (string.IsNullOrEmpty(name))
    {
        if (statusText != null)
            statusText.text = "Vui lòng nhập tên";
        return;
    }

    // 🔥 LƯU LOCAL
    PlayerPrefs.SetString("PlayerName", name);

    // 🔥 GAME MULTIPLAYER (GIỮ NGUYÊN)
    localPlayer.SetPlayerName(name);
    localPlayer.RPC_SetName(name);

    // 🔥 👇 THÊM DÒNG NÀY (QUAN TRỌNG NHẤT)
    PlayFabManager.Instance.SetPlayerName(name);

    localPlayer.SetControl(true);
    // 🔥 PHÁT NHẠC NỀN
if (bgmSource != null && bgmClip != null)
{
    bgmSource.clip = bgmClip;
    bgmSource.loop = true;
    bgmSource.Play();
}   

    panel.SetActive(false);
}
}
using UnityEngine;
using TMPro;

public class NameInputUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject panel;

    [Header("Status UI")]
    public TextMeshProUGUI statusText;

    private PlayerController localPlayer;

    void Start()
    {
        panel.SetActive(true);

        if (statusText != null)
            statusText.text = "Đang kết nối server...";
    }

    [System.Obsolete]
    void Update()
    {
        // tìm player
        if (localPlayer == null)
        {
            foreach (var p in FindObjectsOfType<PlayerController>())
            {
                if (p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    localPlayer.SetControl(false);

                    Debug.Log("FOUND LOCAL PLAYER");

                    // ✅ cập nhật UI
                    if (statusText != null)
                        statusText.text = "Đã kết nối! Nhập tên của bạn";

                    break;
                }
            }
        }
    }

    [System.Obsolete]
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

    // 🔥 LƯU NAME
    PlayerPrefs.SetString("PlayerName", name);

    localPlayer.SetPlayerName(name);
    localPlayer.RPC_SetName(name);

    localPlayer.SetControl(true);

    panel.SetActive(false);
}
}
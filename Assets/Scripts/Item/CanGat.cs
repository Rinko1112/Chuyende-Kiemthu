using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class CanGat : NetworkBehaviour
{
    [Header("CỬA LIÊN KẾT")]
    [SerializeField] private CuaNhieuNut cua;

    [Header("UI PREFAB")]
    [SerializeField] private GameObject interactUIPrefab;

    private GameObject currentUI;
    private Button uiButton;
    private TMP_Text uiText;

    [Header("TEXT")]
    [SerializeField] private string textBat = "F - Bật cần gạt";
    [SerializeField] private string textDaBat = "Đã bật";

    [Header("MODE")]
    [SerializeField] private bool requireHold = false; // 🔥 CHECKBOX

    [Networked] public NetworkBool DaBat { get; set; }

    private PlayerController currentPlayer;
    private bool playerTrongVung = false;

    private bool isSpawned = false;

    public override void Spawned()
    {
        isSpawned = true;

        if (interactUIPrefab != null)
        {
            currentUI = Instantiate(interactUIPrefab);
            currentUI.SetActive(false);

            uiButton = currentUI.GetComponentInChildren<Button>();
            uiText = currentUI.GetComponentInChildren<TMP_Text>();

            if (uiButton != null)
            {
                uiButton.onClick.RemoveAllListeners();
                uiButton.onClick.AddListener(OnClickUI);
            }
        }
    }

    void Update()
    {
        if (!isSpawned) return;

        // ===== UI FOLLOW =====
        if (currentUI != null)
        {
            currentUI.transform.position = transform.position + Vector3.up * 2f;

            if (Camera.main != null)
                currentUI.transform.forward = Camera.main.transform.forward;
        }

        // ===== INPUT =====
        if (currentPlayer != null && currentPlayer.Object.HasInputAuthority)
        {
            if (currentUI != null)
                currentUI.SetActive(playerTrongVung);

            UpdateUIText();

            if (playerTrongVung)
            {
                if (requireHold)
                {
                    // 🔥 HOLD MODE
                    if (Input.GetKey(KeyCode.F))
                    {
                        if (!DaBat)
                            BatCanGat(true);
                    }
                    else
                    {
                        if (DaBat)
                            BatCanGat(false);
                    }
                }
                else
                {
                    // 🔥 NORMAL MODE (GIỮ NGUYÊN)
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (!DaBat)
                            BatCanGat(true);
                    }
                }
            }
        }
    }

    void UpdateUIText()
    {
        if (uiText == null) return;

        if (requireHold)
        {
            uiText.text = DaBat ? "Đang giữ..." : "Giữ F";
        }
        else
        {
            uiText.text = DaBat ? textDaBat : textBat;
        }
    }

    void OnClickUI()
    {
        if (currentPlayer == null) return;
        if (!currentPlayer.Object.HasInputAuthority) return;

        if (requireHold)
        {
            BatCanGat(!DaBat); // fallback mobile
        }
        else
        {
            if (!DaBat)
                BatCanGat(true);
        }
    }

    void BatCanGat(bool state)
    {
        if (!Object.HasStateAuthority)
        {
            RPC_BatCanGat(state);
            return;
        }

        ThucHien(state);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_BatCanGat(bool state)
    {
        ThucHien(state);
    }

    void ThucHien(bool state)
    {
        if (DaBat == state) return;

        DaBat = state;

        // 🔥 báo cửa
        if (cua != null)
        {
            cua.NotifyPlateChanged();
        }

        RPC_CapNhatTrangThai(DaBat);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_CapNhatTrangThai(bool state)
    {
        DaBat = state;
    }

    // ===== TRIGGER =====
    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        playerTrongVung = true;
        currentPlayer = player;
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (currentPlayer == player)
        {
            playerTrongVung = false;

            // 🔥 nếu là HOLD → thả ra là tắt
            if (requireHold && DaBat)
                BatCanGat(false);
        }
    }

    public bool GetState()
    {
        return DaBat;
    }
}
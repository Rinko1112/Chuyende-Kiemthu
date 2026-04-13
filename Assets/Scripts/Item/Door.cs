using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class Door : NetworkBehaviour
{
    [Header("CONFIG")]
    [SerializeField] private string requiredItem = "key";
    [SerializeField] private float openHeight = 2f;
    [SerializeField] private float openSpeed = 2f;

    [Header("UI PREFAB")]
    [SerializeField] private GameObject interactUIPrefab;

    private GameObject currentUI;
    private Button uiButton;
    private TMP_Text uiText;

    [Header("TEXT")]
    [SerializeField] private string textOpen = "F - Mở cửa";
    [SerializeField] private string textLocked = "Cần chìa khóa";

    private Collider col;

    [Networked] private NetworkBool IsOpen { get; set; }

    private Vector3 closedPos;
    private Vector3 openPos;

    private PlayerStats currentPlayer;
    private bool playerInRange = false;

    private bool isSpawned = false;

    public override void Spawned()
    {
        isSpawned = true;

        closedPos = transform.position;
        openPos = closedPos + Vector3.up * openHeight;

        col = GetComponent<Collider>();

        // 🔥 AUTO SPAWN UI
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

        // ===== MOVE DOOR =====
        Vector3 target = IsOpen ? openPos : closedPos;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            Time.deltaTime * openSpeed
        );

        if (IsOpen && col != null)
            col.enabled = false;

        // ===== UI FOLLOW =====
        if (currentUI != null)
        {
            currentUI.transform.position = transform.position + Vector3.up * 2.5f;

            if (Camera.main != null)
                currentUI.transform.forward = Camera.main.transform.forward;
        }

        // ===== INPUT =====
        if (!playerInRange || currentPlayer == null) return;
        if (!currentPlayer.Object.HasInputAuthority) return;
        if (IsOpen) return;

        bool hasKey = HasItem(currentPlayer, requiredItem);

        // 🔥 HIỆN UI
        if (currentUI != null)
            currentUI.SetActive(true);

        UpdateUIText(hasKey);

        if (hasKey && Input.GetKeyDown(KeyCode.F))
        {
            TryOpenDoor(currentPlayer);
        }
    }

    void UpdateUIText(bool hasKey)
    {
        if (uiText == null) return;

        if (hasKey)
            uiText.text = textOpen;
        else
            uiText.text = textLocked;
    }

    void OnClickUI()
    {
        if (currentPlayer == null) return;
        if (!currentPlayer.Object.HasInputAuthority) return;

        bool hasKey = HasItem(currentPlayer, requiredItem);

        if (hasKey)
            TryOpenDoor(currentPlayer);
    }

    bool HasItem(PlayerStats player, string itemId)
    {
        var items = player.GetInventory();

        foreach (var item in items)
        {
            if (item == itemId)
                return true;
        }

        return false;
    }

    void RemoveItem(PlayerStats player, string itemId)
    {
        if (!player.Object.HasStateAuthority)
        {
            player.RPC_RequestRemoveItem(itemId);
            return;
        }

        var items = player.GetInventory();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemId)
            {
                player.SetInventorySlot(i, "");
                return;
            }
        }
    }

    void TryOpenDoor(PlayerStats player)
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestOpen(player.Object);
            return;
        }

        OpenDoor(player);
    }

    void OpenDoor(PlayerStats player)
    {
        if (IsOpen) return;

        RemoveItem(player, requiredItem);
        IsOpen = true;

        if (currentUI != null)
            currentUI.SetActive(false);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestOpen(NetworkObject playerObj)
    {
        if (playerObj == null) return;

        PlayerStats player = playerObj.GetComponent<PlayerStats>();
        if (player == null) return;

        OpenDoor(player);
    }

    // ===== TRIGGER =====
    public void OnPlayerEnter(Collider other)
    {
        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        playerInRange = true;
        currentPlayer = player;
    }

    public void OnPlayerExit(Collider other)
    {
        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        if (currentPlayer == player)
        {
            playerInRange = false;
            currentPlayer = null;

            if (currentUI != null)
                currentUI.SetActive(false);
        }
    }
}
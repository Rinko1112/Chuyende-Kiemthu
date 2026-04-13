using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Fusion;

public class TuiDo : MonoBehaviour
{
    [Header("PANEL")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("BUTTON")]
    [SerializeField] private GameObject openButton;

    [Header("ANIMATION")]
    [SerializeField] private float animSpeed = 6f;

    [Header("SLOTS")]
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private TextMeshProUGUI[] slotCounts;

    [Header("ITEM ICON DB")]
    [SerializeField] private ItemIconData[] itemDatabase;

    private RectTransform rect;
    private bool isOpen = false;

    private Vector3 hiddenScale = new Vector3(0, 0, 0);
    private Vector3 showScale = Vector3.one;

    private PlayerStats localPlayer;

    [System.Serializable]
    public class ItemIconData
    {
        public string itemId;
        public Sprite icon;
    }

    void Start()
    {
        rect = inventoryPanel.GetComponent<RectTransform>();

        inventoryPanel.SetActive(false);
        rect.localScale = hiddenScale;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            slotIcons[i].enabled = false;
            slotCounts[i].text = "";
        }
    }

    void Update()
    {
        // ===== tìm local player =====
        if (localPlayer == null)
        {
            var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

            foreach (var p in players)
            {
                if (p.Object != null && p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    break;
                }
            }
        }

        // ===== update UI từ network inventory =====
        if (localPlayer != null)
        {
            UpdateInventoryUI();
        }

        // ===== animation =====
        if (inventoryPanel.activeSelf)
        {
            rect.localScale = Vector3.Lerp(
                rect.localScale,
                showScale,
                Time.deltaTime * animSpeed
            );
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }

        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                CloseInventory();
            }
        }
    }

    // ===== UPDATE UI FROM NETWORK =====
    void UpdateInventoryUI()
    {
        string[] items = localPlayer.GetInventory();

        // reset
        for (int i = 0; i < slotIcons.Length; i++)
        {
            slotIcons[i].enabled = false;
            slotCounts[i].text = "";
        }

        // count stack
        System.Collections.Generic.Dictionary<string, int> counts
            = new System.Collections.Generic.Dictionary<string, int>();

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            if (!counts.ContainsKey(item))
                counts[item] = 0;

            counts[item]++;
        }

        int index = 0;

        foreach (var pair in counts)
        {
            if (index >= slotIcons.Length) break;

            Sprite icon = GetIcon(pair.Key);

            slotIcons[index].sprite = icon;
            slotIcons[index].enabled = true;

            slotCounts[index].text = pair.Value > 1 ? pair.Value.ToString() : "";

            index++;
        }
    }

    Sprite GetIcon(string itemId)
    {
        foreach (var item in itemDatabase)
        {
            if (item.itemId == itemId)
                return item.icon;
        }

        return null;
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        rect.localScale = hiddenScale;

        isOpen = true;

        if (openButton != null)
            openButton.SetActive(false);

        if (PlayerController.Local != null)
            PlayerController.Local.SetControl(false);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);

        isOpen = false;

        if (openButton != null)
            openButton.SetActive(true);

        if (PlayerController.Local != null)
            PlayerController.Local.SetControl(true);
    }

    public void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }
}
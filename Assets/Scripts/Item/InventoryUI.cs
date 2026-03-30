
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("PANEL")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("BUTTON")]
    [SerializeField] private GameObject openButton;

    [Header("ANIMATION")]
    [SerializeField] private float animSpeed = 6f;

    private RectTransform rect;
    private bool isOpen = false;

    private Vector3 hiddenScale = new Vector3(0, 0, 0);
    private Vector3 showScale = Vector3.one;

    void Start()
    {
        rect = inventoryPanel.GetComponent<RectTransform>();

        inventoryPanel.SetActive(false);
        rect.localScale = hiddenScale;
    }

    void Update()
    {
        // ===== ANIMATION =====
        if (inventoryPanel.activeSelf)
        {
            rect.localScale = Vector3.Lerp(
                rect.localScale,
                showScale,
                Time.deltaTime * animSpeed
            );
        }

        // ===== ESC =====
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }

        // ===== CLICK NGOÀI =====
        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                CloseInventory();
            }
        }
    }

    // ===== OPEN =====
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

    // ===== CLOSE =====
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);

        isOpen = false;

        if (openButton != null)
            openButton.SetActive(true);

        if (PlayerController.Local != null)
            PlayerController.Local.SetControl(true);
    }

    // ===== TOGGLE =====
    public void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }
}

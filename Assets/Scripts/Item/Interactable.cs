using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [Header("UI PREFAB")]
    [SerializeField] private GameObject interactUIPrefab;

    private GameObject currentUI;
    private Button button;

    protected virtual void Start()
    {
        if (interactUIPrefab != null)
        {
            currentUI = Instantiate(interactUIPrefab);
            currentUI.SetActive(false);

            button = currentUI.GetComponentInChildren<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnInteract);
            }
        }
    }

    protected virtual void Update()
    {
        if (currentUI != null)
        {
            // UI follow object
            currentUI.transform.position = transform.position + Vector3.up * 2f;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerController>()) return;

        if (currentUI != null)
            currentUI.SetActive(true);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<PlayerController>()) return;

        if (currentUI != null)
            currentUI.SetActive(false);
    }

    protected virtual void OnInteract()
    {
        Debug.Log("Interact!");
    }
    
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class HPBarWorld : MonoBehaviour
{
    public Slider slider;
    public Slider staminaSlider;
    public Transform target;

    [Header("NAME")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("HOST ICON")]
    public GameObject hostIcon;

    private Camera cam;
    private PlayerStats stats;
    private NetworkObject netObj;
    private PlayerController controller;

    private float smoothSpeed = 10f;

    void Start()
    {
        cam = Camera.main;

        if (target != null)
        {
            stats = target.GetComponent<PlayerStats>();
            netObj = target.GetComponent<NetworkObject>();
            controller = target.GetComponent<PlayerController>();
        }

        slider.maxValue = 100;
        slider.value = 100;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = 100;
            staminaSlider.value = 100;
        }

        // 🔥 HOST CHECK
        if (hostIcon != null && netObj != null)
        {
            bool isHost = netObj.InputAuthority.PlayerId == 1;
            hostIcon.SetActive(isHost);
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // ===== FOLLOW =====
        transform.position = target.position + Vector3.up * 2.5f;
        transform.forward = cam.transform.forward;

        // ===== HP / STAMINA =====
        if (stats != null)
        {
            slider.value = Mathf.Lerp(
                slider.value,
                stats.HP,
                Time.deltaTime * smoothSpeed
            );

            if (staminaSlider != null)
            {
                staminaSlider.value = Mathf.Lerp(
                    staminaSlider.value,
                    stats.Stamina,
                    Time.deltaTime * smoothSpeed
                );
            }

            if (stats.HP <= 0)
            {
                Destroy(gameObject);
            }
        }

        // ===== NAME (GỘP TẠI ĐÂY) =====
        if (controller != null && nameText != null)
        {
            nameText.text = controller.GetPlayerName();
        }
    }
}
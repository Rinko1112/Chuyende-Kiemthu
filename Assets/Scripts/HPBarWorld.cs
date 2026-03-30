using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class HPBarWorld : MonoBehaviour
{
    public Slider slider;
    public Slider staminaSlider;
    public Transform target;

    [Header("HOST ICON")]
    public GameObject hostIcon;

    private Camera cam;
    private PlayerStats stats;
    private NetworkObject netObj;

    private float smoothSpeed = 10f;

    void Start()
    {
        cam = Camera.main;

        if (target != null)
        {
            stats = target.GetComponent<PlayerStats>();
            netObj = target.GetComponent<NetworkObject>();
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

        transform.position = target.position + Vector3.up * 2f;
        transform.forward = cam.transform.forward;

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
    }
}
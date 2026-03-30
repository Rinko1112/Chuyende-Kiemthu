using UnityEngine;
using UnityEngine.UI;

public class UIButtonSoundManager : MonoBehaviour
{
    public static UIButtonSoundManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupAllButtons();
    }

    public void SetupAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // true = include inactive

        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(PlayClick); // tránh bị add nhiều lần
            btn.onClick.AddListener(PlayClick);
        }

        Debug.Log("✅ All buttons have sound!");
    }

    void PlayClick()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
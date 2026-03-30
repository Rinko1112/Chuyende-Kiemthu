using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("PANELS")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    void Awake()
    {
        Instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }

    public void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);
    }

    public void ShowLose()
    {
        if (losePanel != null)
            losePanel.SetActive(true);

        if (audioSource != null && loseSound != null)
            audioSource.PlayOneShot(loseSound);
    }
}
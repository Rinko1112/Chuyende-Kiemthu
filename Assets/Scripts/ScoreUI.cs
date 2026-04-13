using UnityEngine;
using TMPro;
using Fusion;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private PlayerStats localPlayer;

    [System.Obsolete]
    void Update()
    {
        // 🔥 tìm đúng player có InputAuthority
        if (localPlayer == null)
        {
            foreach (var p in FindObjectsOfType<PlayerStats>())
            {
                if (p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    break;
                }
            }
            return;
        }

        scoreText.text = " " + localPlayer.Score;
    }
}
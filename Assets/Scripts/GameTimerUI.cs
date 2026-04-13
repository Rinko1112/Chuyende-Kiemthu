using UnityEngine;
using TMPro;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    void Update()
{
    if (GameManager.Instance == null) return;

    var gm = GameManager.Instance;

    // 🔥 FIX QUAN TRỌNG
    if (gm.Object == null || !gm.Object.IsValid) return;

    float time = gm.GetTimeRemaining();

    int minutes = Mathf.FloorToInt(time / 60f);
    int seconds = Mathf.FloorToInt(time % 60f);

    timeText.text = $"{minutes:00}:{seconds:00}";
}
}
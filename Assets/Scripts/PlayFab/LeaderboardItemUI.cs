using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardItemUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    public void Setup(int rank, string name, int score, int time, bool isMe)
    {
        rankText.text = rank.ToString();
        nameText.text = name;
        scoreText.text = score.ToString();
        timeText.text = time + "s";

        if (isMe)
            GetComponent<Image>().color = Color.yellow;
    }
}
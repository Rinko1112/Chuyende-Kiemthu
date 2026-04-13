using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkill : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("CONFIG")]
    [SerializeField] private string skillName; // ví dụ: "HealSkill"

    private MonoBehaviour skill;
    

    void Start()
    {
        FindSkill();
    }

    void Update()
    {
        if (skill == null)
        {
            FindSkill();
            return;
        }

        // 👉 dùng reflection nhẹ để gọi function chung
        var type = skill.GetType();

        var getPercent = type.GetMethod("GetCooldownPercent");
        var getRemain = type.GetMethod("GetCooldownRemaining");

        if (getPercent == null || getRemain == null) return;

        float percent = (float)getPercent.Invoke(skill, null);
        float remain = (float)getRemain.Invoke(skill, null);

        // ===== UPDATE UI =====
        if (cooldownFill != null)
            cooldownFill.fillAmount = percent;

        if (cooldownText != null)
            cooldownText.text = remain > 0 ? Mathf.CeilToInt(remain).ToString() : "";

        // ===== ICON GRAY WHEN CD =====
        if (icon != null)
        {
            icon.color = remain > 0
                ? new Color(1f, 1f, 1f, 0.5f)
                : Color.white;
        }
    }

    void FindSkill()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.Object != null && p.Object.HasInputAuthority)
            {
                var comp = p.GetComponent(skillName);

                if (comp != null)
                {
                    skill = comp as MonoBehaviour;
                    return;
                }
            }
        }
    }
}
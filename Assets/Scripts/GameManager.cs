using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Networked] public bool IsGameEnded { get; set; }

    [Header("TIMER")]
    [SerializeField] private float matchDuration = 1800f;

    [Networked] private float StartTime { get; set; }
    [Networked] private bool IsTimerRunning { get; set; }

    void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            IsTimerRunning = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (IsGameEnded) return;

        if (IsTimerRunning)
        {
            float elapsed = Runner.SimulationTime - StartTime;

            if (elapsed >= matchDuration)
            {
                TriggerLose();
            }
        }
    }

    // ================= TIMER =================
    public void StartTimer()
    {
        if (!Object.HasStateAuthority) return;

        StartTime = Runner.SimulationTime;
        IsTimerRunning = true;
    }

    public int GetElapsedTime()
    {
        if (!IsTimerRunning) return 0;

        float elapsed = Runner.SimulationTime - StartTime;
        return Mathf.RoundToInt(elapsed);
    }

    public float GetTimeRemaining()
    {
        if (!IsTimerRunning) return matchDuration;

        float elapsed = Runner.SimulationTime - StartTime;
        float remain = matchDuration - elapsed;

        return Mathf.Max(0f, remain);
    }

    // ================= WIN =================
    public void TriggerWin()
    {
        if (IsGameEnded) return;

        IsGameEnded = true;

        RPC_GameWin();

        // 🔥 QUAN TRỌNG: gọi RPC để mọi client tự submit
        RPC_SubmitScore(GetElapsedTime());
    }

    // ================= LOSE =================
    void TriggerLose()
    {
        if (IsGameEnded) return;

        IsGameEnded = true;
        Debug.Log("YOU LOSE");
    }

    // ================= RPC =================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_GameWin()
    {
        Debug.Log("YOU WIN");
    }

    // 🔥 FIX CHÍNH Ở ĐÂY
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SubmitScore(int time)
    {
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p == null) continue;

            // 🔥 mỗi client chỉ submit CHÍNH NÓ
            if (p.Object != null && p.Object.HasInputAuthority)
            {
                int score = p.Score;

                Debug.Log($"🔥 CLIENT SUBMIT | Score={score} Time={time}");

                PlayFabManager.Instance?.SubmitScore(score, time);
            }
        }
    }
}
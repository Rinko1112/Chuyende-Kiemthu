using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Networked] public bool IsGameEnded { get; set; }

    void Awake()
    {
        Instance = this;
    }

    [System.Obsolete]
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (IsGameEnded) return;

        CheckLoseCondition();
    }

    // ===== LOSE =====
    [System.Obsolete]
    void CheckLoseCondition()
    {
        var players = FindObjectsOfType<PlayerStats>();

        if (players.Length == 0) return;

        bool allDead = true;

        foreach (var p in players)
        {
            if (p == null) continue;

            if (p.HP > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            TriggerLose();
        }
    }

    public void TriggerLose()
    {
        if (IsGameEnded) return;

        IsGameEnded = true;

        RPC_GameLose();
    }

    public void TriggerWin()
    {
        if (IsGameEnded) return;

        IsGameEnded = true;

        RPC_GameWin();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_GameWin()
{
    Debug.Log("YOU WIN");

    if (GameUIManager.Instance != null)
        GameUIManager.Instance.ShowWin();
}

[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_GameLose()
{
    Debug.Log("YOU LOSE");

    if (GameUIManager.Instance != null)
        GameUIManager.Instance.ShowLose();
}
}
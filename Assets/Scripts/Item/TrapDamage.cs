using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class TrapDamage : NetworkBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private float cooldown = 1f;

    private float lastHitTime;
    private Animator anim;

    [Networked] private bool IsOpen { get; set; }

    // 🔥 list player đang đứng trong trap
    private List<PlayerStats> playersInTrap = new List<PlayerStats>();

    public override void Spawned()
    {
        anim = GetComponent<Animator>();

        if (Object.HasStateAuthority)
        {
            InvokeRepeating(nameof(OpenTrap), 0f, 4f);
            InvokeRepeating(nameof(CloseTrap), 2f, 4f);
        }
    }

    void OpenTrap()
    {
        RPC_Open();
    }

    void CloseTrap()
    {
        RPC_Close();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Open()
    {
        anim?.SetTrigger("open");
        IsOpen = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Close()
    {
        anim?.SetTrigger("close");
        IsOpen = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsOpen) return;

        if (Runner.SimulationTime - lastHitTime < cooldown) return;
        lastHitTime = Runner.SimulationTime;

        foreach (var player in playersInTrap)
        {
            if (player == null) continue;

            player.RPC_TakeDamage(damage);

            Debug.Log("🔥 TRAP HIT: " + player.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var stats = other.GetComponentInParent<PlayerStats>();
        if (stats != null && !playersInTrap.Contains(stats))
        {
            playersInTrap.Add(stats);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var stats = other.GetComponentInParent<PlayerStats>();
        if (stats != null)
        {
            playersInTrap.Remove(stats);
        }
    }
}
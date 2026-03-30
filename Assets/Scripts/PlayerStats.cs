using System;
using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : NetworkBehaviour
{
    [Networked] public int HP { get; set; }
    [Networked] public float Speed { get; set; }
    [Networked] public float Stamina { get; set; }

    // 🔥 SCORE
    [Networked] public int Score { get; set; }

    public static List<PlayerRef> DeadPlayers = new List<PlayerRef>();

    [Header("STAMINA")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 15f;
    public float staminaRegenPerSecond = 10f;

    private float baseSpeed = 4f;
    private float runBonus = 4f;

    private Animator animator;
    private PlayerController controller;

    private bool isDead = false;
    private bool isRunning = false;
    private bool isExhausted = false;

    // ===== HURT EFFECT =====
    [Header("HURT EFFECT")]
    [SerializeField] private float hurtFlashDuration = 0.2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetDead(string data)
    {
        if (PlayerListManager.Instance != null)
        {
            PlayerListManager.Instance.SetFullData(data);
        }
    }

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();

        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();

        if (Object.HasStateAuthority)
        {
            HP = 100;
            Speed = baseSpeed;
            Stamina = maxStamina;

            Score = 0; // 🔥 reset score
        }

        isDead = false;
        isRunning = false;
        isExhausted = false;
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (isRunning && Stamina > 0)
        {
            Stamina -= staminaDrainPerSecond * Runner.DeltaTime;

            if (Stamina <= 0)
            {
                Stamina = 0;
                StopRunning();
                isExhausted = true;
            }
        }
        else
        {
            if (Stamina < maxStamina)
            {
                Stamina += staminaRegenPerSecond * Runner.DeltaTime;

                if (Stamina >= maxStamina)
                {
                    Stamina = maxStamina;
                    isExhausted = false;
                }
            }
        }
    }

    public void StartRunning()
    {
        if (!Object.HasStateAuthority) return;
        if (isExhausted) return;
        if (Stamina <= 0) return;

        isRunning = true;
        Speed = baseSpeed + runBonus;
    }

    public void StopRunning()
    {
        if (!Object.HasStateAuthority) return;

        isRunning = false;
        Speed = baseSpeed;
    }

    public bool IsRunning() => isRunning;
    public bool IsExhausted() => isExhausted;

    // ===== SCORE =====
    public void AddScore(int amount)
    {

        Score += amount;
    }

    // ===== DAMAGE =====
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount)
    {
        ApplyDamage(amount);
    }

    public void ApplyDamage(int amount)
    {
        if (!Object.HasStateAuthority) return;
        if (isDead) return;

        HP -= amount;

        if (HP <= 0)
        {
            HP = 0;
            Die();
            return;
        }

        RPC_PlayHurt();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayHurt()
    {
        if (animator != null)
            animator.SetTrigger("Hurt");

        StartCoroutine(HurtFlash());
    }

    System.Collections.IEnumerator HurtFlash()
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", Color.red);
            r.SetPropertyBlock(mpb);
        }

        yield return new WaitForSeconds(hurtFlashDuration);

        foreach (var r in renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", Color.white);
            r.SetPropertyBlock(mpb);
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        DeadPlayers.Add(Object.InputAuthority);

        RPC_PlayDie();

        if (controller != null)
        {
            controller.SetDead(true);
        }

        if (Object.HasInputAuthority)
        {
            if (SpectatorCamera.Instance != null)
                SpectatorCamera.Instance.ActivateSpectator();
        }

        if (Object.HasStateAuthority && controller != null)
        {
            int id = Object.InputAuthority.PlayerId;

            PlayerListManager.Instance.SetDead(id, true);

            RPC_SetDead(PlayerListManager.Instance.GetRawData());
        }

        if (SpectatorCamera.Instance != null)
        {
            SpectatorCamera.Instance.RemovePlayer(transform);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayDie()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    void DespawnPlayer()
    {
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_AddScore(int amount)
{
    Score += amount;
}
[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
public void RPC_Revive(Vector3 pos)
{
    isDead = false;

    HP = 100;
    Stamina = maxStamina;

    if (controller != null)
    {
        controller.SetDead(false);
    }

    transform.position = pos;

    if (animator != null)
    {
        animator.Rebind();
        animator.Update(0f);
    }
}
public void Revive(Vector3 pos)
{
    if (!Object.HasStateAuthority) return;

    RPC_Revive(pos);

    int id = Object.InputAuthority.PlayerId;

    PlayerListManager.Instance.SetDead(id, false);
    RPC_SetDead(PlayerListManager.Instance.GetRawData());
}

}
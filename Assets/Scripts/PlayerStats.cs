using System;
using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : NetworkBehaviour
{
    private int lastHurtIndex = -1;
    [Networked] public int HP { get; set; }
    [Networked] public float Speed { get; set; }
    [Networked] public float Stamina { get; set; }

    [Networked] public int Score { get; set; }

    public static List<PlayerRef> DeadPlayers = new List<PlayerRef>();

    [Header("STAMINA")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 15f;
    public float staminaRegenPerSecond = 10f;
    [SerializeField] private float normalRegenRate = 2f;   // đứng (chậm)
[SerializeField] private float sittingRegenRate = 10f; // ngồi (nhanh)

    private float baseSpeed = 4f;
    private float runBonus = 4f;

    private Animator animator;
    private PlayerController controller;

    [Networked] private NetworkBool isDead { get; set; }
    private bool isRunning = false;
    private bool isExhausted = false;

    [Header("HURT EFFECT")]
    [Header("HURT SOUND")]
[SerializeField] private AudioSource hurtAudioSource;
[SerializeField] private AudioClip[] hurtSounds; // kéo 5 sound vào đây
    [SerializeField] private float hurtFlashDuration = 0.2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    // 🔥 HEAL EFFECT
    [Header("HEAL EFFECT")]
    [SerializeField] private GameObject healAuraPrefab;
    private GameObject healAuraInstance;
    private float lastHealTime;

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
            Score = 0;
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
                float regen = staminaRegenPerSecond;

regen = isSitting ? sittingRegenRate : normalRegenRate;

Stamina += regen * Runner.DeltaTime;

                if (Stamina >= maxStamina)
                {
                    Stamina = maxStamina;
                    isExhausted = false;
                }
            }
        }
    }

    void Update()
    {
        // 🔥 tắt aura nếu không còn heal sau 1s
        if (healAuraInstance != null && Time.time - lastHealTime > 0.5f)
        {
            Destroy(healAuraInstance);
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

    public void AddScore(int amount)
    {
        Score += amount;
    }

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

if (Object.HasInputAuthority)
{
    if (hurtAudioSource != null && hurtSounds != null && hurtSounds.Length > 0)
    {
        int index = UnityEngine.Random.Range(0, hurtSounds.Length);

        // 🔥 tránh trùng sound liên tiếp
        if (hurtSounds.Length > 1 && index == lastHurtIndex)
        {
            index = (index + 1) % hurtSounds.Length;
        }

        lastHurtIndex = index;

        hurtAudioSource.PlayOneShot(hurtSounds[index]);
    }
}

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

    if (controller != null)
        controller.SetDead(true); // 🔥 THÊM DÒNG NÀY
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

    // 🔥 HEAL + AURA
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddHeal(float amount)
    {
        if (HP <= 0) return;

        HP += Mathf.RoundToInt(amount);

        if (HP > 100)
            HP = 100;

        RPC_PlayHealEffect();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayHealEffect()
    {
        lastHealTime = Time.time;

        if (healAuraPrefab == null) return;

        if (healAuraInstance == null)
        {
            healAuraInstance = Instantiate(healAuraPrefab, transform);
            healAuraInstance.transform.localPosition = Vector3.zero;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetDead(string data)
    {
        if (PlayerListManager.Instance != null)
        {
            PlayerListManager.Instance.SetFullData(data);
        }
    }
    // ===== INVENTORY SYSTEM =====
[Networked, Capacity(10)]
private NetworkArray<NetworkString<_16>> Inventory => default;

// ===== ADD ITEM =====
[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_AddItem(string itemId)
{
    for (int i = 0; i < Inventory.Length; i++)
    {
        if (string.IsNullOrEmpty(Inventory[i].ToString()))
        {
            Inventory.Set(i, itemId);
            return;
        }
    }
}

// ===== GET INVENTORY =====
public string[] GetInventory()
{
    string[] items = new string[Inventory.Length];

    for (int i = 0; i < Inventory.Length; i++)
    {
        items[i] = Inventory[i].ToString();
    }

    return items;
}
// ===== REMOVE ITEM =====
[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_RequestRemoveItem(string itemId)
{
    for (int i = 0; i < Inventory.Length; i++)
    {
        if (Inventory[i].ToString() == itemId)
        {
            Inventory.Set(i, "");
            return;
        }
    }
}

// ===== SET SLOT (dùng nội bộ) =====
public void SetInventorySlot(int index, string value)
{
    Inventory.Set(index, value);
}
private bool isSitting = false;
[SerializeField] private float sittingRegenMultiplier = 2.5f;
public void SetSitting(bool value)
{
    isSitting = value;
}

}
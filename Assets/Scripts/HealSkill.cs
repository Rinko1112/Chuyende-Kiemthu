using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class HealSkill : NetworkBehaviour
{
    [Header("HEAL")]
    [SerializeField] private int healPerSecond = 20;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float radius = 3f;

    [Header("COOLDOWN")]
    [SerializeField] private float cooldown = 180f; // 3 phút

    [Header("EFFECT")]
    [SerializeField] private ParticleSystem healEffect;

    [Networked] private NetworkBool IsHealing { get; set; }
    [Header("SOUND")]
[SerializeField] private AudioSource audioSource;
[SerializeField] private AudioClip healSound;
[SerializeField] private Animator animator;
[SerializeField] private float healStartDelay = 0.5f;
[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_PlayHealAnim()
{
    if (animator != null)
    {
        animator.SetTrigger("Heal");
    }
}

    private float lastUseTime = -999f;
    private float timer;
    private float healTickTimer;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (IsHealing)
        {
            timer += Runner.DeltaTime;

            HealPlayers();

            if (timer >= duration)
            {
                IsHealing = false;
                RPC_PlayEffect(false);
            }
        }
    }

    void Update()
    {
        if (!Object || !Object.HasInputAuthority) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryUseSkill();
        }
    }

    void TryUseSkill()
    {
        if (Time.time - lastUseTime < cooldown) return;

        lastUseTime = Time.time;

        RPC_StartHeal();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
void RPC_StartHeal()
{
    // 🔥 chạy animation trước
    RPC_PlayHealAnim();

    // 🔥 delay rồi mới heal
    StartCoroutine(StartHealDelayed());
}

    float tickRate = 0.2f; // 🔥 5 lần / giây

void HealPlayers()
{
    healTickTimer += Runner.DeltaTime;

    if (healTickTimer < tickRate) return;

    healTickTimer = 0f;

    int healAmount = Mathf.RoundToInt(healPerSecond * tickRate); // 20 * 0.2 = 4

    Collider[] hits = Physics.OverlapSphere(transform.position, radius);

    foreach (var hit in hits)
    {
        PlayerStats stats = hit.GetComponentInParent<PlayerStats>();

        if (stats == null) continue;
        if (stats.HP <= 0) continue;

        stats.RPC_AddHeal(healAmount);
    }
}

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_PlayEffect(bool on)
{
    if (healEffect != null)
    {
        if (on) healEffect.Play();
        else healEffect.Stop();
    }

    // 🔊 PLAY SOUND
    if (on && audioSource != null && healSound != null)
    {
        audioSource.PlayOneShot(healSound);
    }
}

    public override void Render()
    {
        if (!IsHealing)
        {
            if (healEffect != null && healEffect.isPlaying)
                healEffect.Stop();
        }
    }
    public float GetCooldownRemaining()
{
    float remain = cooldown - (Time.time - lastUseTime);
    return Mathf.Max(0f, remain);
}

public float GetCooldownPercent()
{
    return GetCooldownRemaining() / cooldown;
}
System.Collections.IEnumerator StartHealDelayed()
{
    yield return new WaitForSeconds(healStartDelay);

    IsHealing = true;
    timer = 0f;
    healTickTimer = 0f;

    RPC_PlayEffect(true);
}
}
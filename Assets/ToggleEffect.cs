using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Fusion;

public class ToggleEffect : NetworkBehaviour
{
    [Header("EFFECT")]
    [SerializeField] private ParticleSystem effectParticle;
    [SerializeField] private AudioSource effectAudio;

    [Header("UI")]
    [SerializeField] private Button toggleButton;

    [Header("OPTION")]
    [SerializeField] private float cooldown = 0.2f;

    [Networked] private NetworkBool IsEffectOn { get; set; }

    private float lastTime;

    void Start()
    {
        // gán button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnTogglePressed);
        }
    }

    void Update()
    {
        // chỉ player local
        if (!Object || !Object.HasInputAuthority)
            return;

        // 🔥 FIX DELAY: đọc input ở Update
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            OnTogglePressed();
        }
    }

    // 👉 dùng chung cho cả F và UI button
    public void OnTogglePressed()
    {
        if (Time.time - lastTime < cooldown)
            return;

        lastTime = Time.time;

        RPC_RequestToggle();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestToggle()
    {
        IsEffectOn = !IsEffectOn;
    }

    public override void Render()
    {
        if (IsEffectOn)
        {
            if (effectParticle != null && !effectParticle.isPlaying)
                effectParticle.Play();

            if (effectAudio != null && !effectAudio.isPlaying)
                effectAudio.Play();
        }
        else
        {
            if (effectParticle != null && effectParticle.isPlaying)
                effectParticle.Stop();

            if (effectAudio != null && effectAudio.isPlaying)
                effectAudio.Stop();
        }
    }
}
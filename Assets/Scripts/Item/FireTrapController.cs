using UnityEngine;
using Fusion;
using System.Collections;

public class FireTrapController : NetworkBehaviour
{
    [Header("HIỆU ỨNG")]
    [SerializeField] private GameObject fireEffect;   // 🔥 lửa
    [SerializeField] private GameObject smokeEffect;  // 💨 khói

    [Header("ÁNH SÁNG")]
    [SerializeField] private Light trapLight; // 💡 đèn chiếu sáng vùng trap

    [Header("CÀI ĐẶT")]
    [SerializeField] private float smokeDelay = 0.5f; // thời gian trễ bật khói

    private Coroutine effectRoutine;
    private bool playerInside = false;

    public override void Spawned()
    {
        // 🔥 Ban đầu tắt hết hiệu ứng
        if (fireEffect != null) fireEffect.SetActive(false);
        if (smokeEffect != null) smokeEffect.SetActive(false);

        // 💡 Tắt đèn ban đầu
        if (trapLight != null) trapLight.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        PlayerController p = other.GetComponentInParent<PlayerController>();
        if (p == null) return;

        playerInside = true;
        RPC_StartEffect();
    }

    void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        PlayerController p = other.GetComponentInParent<PlayerController>();
        if (p == null) return;

        playerInside = false;
        RPC_StopEffect();
    }

    // ===== RPC BẬT HIỆU ỨNG =====
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_StartEffect()
    {
        // 💡 bật đèn ngay
        if (trapLight != null)
            trapLight.enabled = true;

        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        effectRoutine = StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        // 🔥 bật lửa trước
        if (fireEffect != null)
            fireEffect.SetActive(true);

        // 💨 delay rồi bật khói
        yield return new WaitForSeconds(smokeDelay);

        if (smokeEffect != null)
            smokeEffect.SetActive(true);
    }

    // ===== RPC TẮT HIỆU ỨNG =====
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_StopEffect()
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        // 🔥 tắt hiệu ứng
        if (fireEffect != null)
            fireEffect.SetActive(false);

        if (smokeEffect != null)
            smokeEffect.SetActive(false);

        // 💡 tắt đèn
        if (trapLight != null)
            trapLight.enabled = false;
    }

    // 🔥 làm ánh sáng lửa nhấp nháy cho đẹp (không bắt buộc)
    void Update()
    {
        if (trapLight != null && trapLight.enabled)
        {
            trapLight.intensity = 12f + Mathf.Sin(Time.time * 8f) * 2f;
        }
    }
}
using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class NutNhan : NetworkBehaviour
{
    [Header("CỬA LIÊN KẾT")]
    [SerializeField] private CuaNhieuNut door;

    private List<Collider> objectsTrenNut = new List<Collider>();

    [Networked] public NetworkBool DangKichHoat { get; set; }

    [Header("ANIMATION")]
    [SerializeField] private float doSauNhan = 0.1f; // độ lún xuống
    [SerializeField] private float tocDoNhan = 6f;   // tốc độ animation

    private Vector3 viTriBanDau;
    private Vector3 viTriBiNhan;

    private bool isSpawned = false;

    public override void Spawned()
    {
        DangKichHoat = false;

        viTriBanDau = transform.localPosition;
        viTriBiNhan = viTriBanDau + Vector3.down * doSauNhan;

        isSpawned = true;
    }

    void Update()
    {
        if (!isSpawned) return;

        // 🔥 animation mượt
        Vector3 target = DangKichHoat ? viTriBiNhan : viTriBanDau;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            target,
            Time.deltaTime * tocDoNhan
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (LaObjectHopLe(other))
        {
            if (!objectsTrenNut.Contains(other))
                objectsTrenNut.Add(other);

            CapNhatTrangThai();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (objectsTrenNut.Contains(other))
        {
            objectsTrenNut.Remove(other);
            CapNhatTrangThai();
        }
    }

    bool LaObjectHopLe(Collider col)
    {
        if (col.GetComponentInParent<PlayerStats>() != null) return true;
        if (col.GetComponentInParent<ThungBay>() != null) return true;

        return false;
    }

    void CapNhatTrangThai()
    {
        bool trangThaiMoi = objectsTrenNut.Count > 0;

        if (DangKichHoat == trangThaiMoi) return;

        DangKichHoat = trangThaiMoi;

        RPC_UpdateState(DangKichHoat);

        if (door != null)
            door.NotifyPlateChanged();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_UpdateState(bool state)
    {
        DangKichHoat = state;
    }

    public bool GetState()
    {
        return DangKichHoat;
    }
}
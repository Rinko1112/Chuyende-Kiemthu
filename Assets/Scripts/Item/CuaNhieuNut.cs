using UnityEngine;
using Fusion;

public class CuaNhieuNut : NetworkBehaviour
{
    [Header("DANH SÁCH NÚT")]
    [SerializeField] private NutNhan[] danhSachNut;
    [SerializeField] private CanGat[] danhSachCanGat;

    [Header("CẤU HÌNH")]
    [SerializeField] private int soNutCan = 2;

    [Header("DI CHUYỂN")]
    [SerializeField] private float doCaoMo = 2f;
    [SerializeField] private float tocDoMo = 2f;

    [Header("HIỂN THỊ ĐIỀU KIỆN")]
    [SerializeField] private GameObject[] danhSachDen; // 🔥 các đèn

    private Vector3 viTriDong;
    private Vector3 viTriMo;
    private Collider col;

    private bool isSpawned = false;

    [Networked] private NetworkBool DangMo { get; set; }

    public override void Spawned()
    {
        isSpawned = true;

        viTriDong = transform.position;
        viTriMo = viTriDong + Vector3.up * doCaoMo;

        col = GetComponent<Collider>();

        // 🔥 tắt hết đèn ban đầu
        CapNhatDen(0);
    }

    void Update()
    {
        if (!isSpawned) return;

        Vector3 target = DangMo ? viTriMo : viTriDong;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            Time.deltaTime * tocDoMo
        );

        if (DangMo && col != null)
            col.enabled = false;
    }

    public void NotifyPlateChanged()
    {
        if (!Object.HasStateAuthority) return;

        int soNutDangBat = 0;

        foreach (var nut in danhSachNut)
        {
            if (nut != null && nut.GetState())
                soNutDangBat++;
        }

        foreach (var can in danhSachCanGat)
        {
            if (can != null && can.GetState())
                soNutDangBat++;
        }

        // 🔥 update đèn
        RPC_CapNhatDen(soNutDangBat);

        bool shouldOpen = soNutDangBat >= soNutCan;

        if (DangMo == shouldOpen) return;

        DangMo = shouldOpen;

        RPC_SetDoorState(DangMo);
    }

    // ===== ĐÈN =====
    void CapNhatDen(int soLuong)
    {
        for (int i = 0; i < danhSachDen.Length; i++)
        {
            if (danhSachDen[i] != null)
                danhSachDen[i].SetActive(i < soLuong);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_CapNhatDen(int soLuong)
    {
        CapNhatDen(soLuong);
    }

    // ===== CỬA =====
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetDoorState(bool open)
    {
        DangMo = open;

        if (!open && col != null)
            col.enabled = true;
    }
}
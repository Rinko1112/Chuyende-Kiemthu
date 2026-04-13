using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local;

    private CharacterController controller;
    private PlayerInput input;
    private PlayerStats stats;
    private PlayerName tenPlayer;

    private Animator anim;
    private Camera cam;

    [Networked] private NetworkBool daChet { get; set; }
    private bool coTheDieuKhien = true;

    private float vanTocY;
    private float trongLuc = -20f;
    private float lucNhay = 8f;

    private bool nhanNhay;

    [Networked] public NetworkString<_16> TenMang { get; set; }

    // ===== HP BAR =====
    [Header("HP BAR")]
    [SerializeField] private GameObject hpBarPrefab;
    private HPBarWorld hpBar;

    // ===== WEAPON FLOAT =====
    [Header("WEAPON FLOAT")]
    [SerializeField] private Transform vuKhi;
    [SerializeField] private float doNhay = 0.2f;
    [SerializeField] private float tocDoNhay = 2f;

    private Vector3 viTriVuKhiBanDau;

    // ===== PUSH =====
    [Header("DAY VAT")]
    [SerializeField] private float khoangDay = 1f;
    [SerializeField] private LayerMask layerDay;

    private float thoiGianDayCuoi = 0f;
    private float cooldownDay = 0.15f;

    // ===== RPC =====
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Nhay()
    {
        if (anim != null)
            anim.SetTrigger("Jump");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Day()
    {
        if (anim != null)
            anim.SetTrigger("Push");
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            cam = Camera.main;

            string tenLuu = PlayerPrefs.GetString("PlayerName", "");
            if (!string.IsNullOrEmpty(tenLuu))
            {
                RPC_SetName(tenLuu);
            }
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        stats = GetComponent<PlayerStats>();
        tenPlayer = GetComponent<PlayerName>();
        anim = GetComponent<Animator>();

        // HP BAR
        if (hpBarPrefab != null)
        {
            GameObject bar = Instantiate(hpBarPrefab);
            hpBar = bar.GetComponent<HPBarWorld>();
            hpBar.target = transform;
        }

        // Camera spectator
        if (SpectatorCamera.Instance != null)
        {
            SpectatorCamera.Instance.RegisterPlayer(transform);
        }

        // Weapon float
        if (vuKhi != null)
        {
            viTriVuKhiBanDau = vuKhi.localPosition;
        }
    }

    void Update()
    {
        if (!Object || !Object.HasInputAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            nhanNhay = true;
        }

        // vũ khí lơ lửng
        if (vuKhi != null)
        {
            float y = Mathf.Sin(Time.time * tocDoNhay) * doNhay;
            vuKhi.localPosition = viTriVuKhiBanDau + new Vector3(0, y, 0);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (stats == null) return;
        if (!Object.HasStateAuthority) return;

if (daChet)
{
    if (anim != null)
    {
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
        anim.SetBool("IsRunning", false);
    }
    return;
}
        if (IsSitting) return;

        Vector2 inputMove = Vector2.zero;
        Vector3 huongDiChuyen = Vector3.zero;

        if (coTheDieuKhien)
        {
            inputMove = input.actions["Move"].ReadValue<Vector2>();
            huongDiChuyen = new Vector3(inputMove.x, 0, inputMove.y);

            if (huongDiChuyen.magnitude > 1f)
                huongDiChuyen = huongDiChuyen.normalized;

            // ===== ĐẨY THÙNG =====
            if (huongDiChuyen.magnitude > 0.1f)
            {
                Vector3 dir = huongDiChuyen.normalized;

                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, khoangDay, layerDay))
                {
                    Thung thung = hit.collider.GetComponent<Thung>();

                    if (thung != null && !thung.DangDiChuyen())
                    {
                        Quaternion rot = Quaternion.LookRotation(dir);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            rot,
                            Runner.DeltaTime * 15f
                        );

                        thung.DiChuyen(dir);

                        if (Time.time - thoiGianDayCuoi > cooldownDay)
                        {
                            RPC_Day();
                            thoiGianDayCuoi = Time.time;
                        }

                        return;
                    }
                }
            }

            // ===== CHẠY =====
            bool giuShift = Keyboard.current.leftShiftKey.isPressed;

            if (giuShift && huongDiChuyen.magnitude > 0.1f && !stats.IsExhausted())
                stats.StartRunning();
            else
                stats.StopRunning();
        }

        // ===== GRAVITY =====
        if (controller.isGrounded)
        {
            if (vanTocY < 0)
                vanTocY = -2f;

            if (coTheDieuKhien && nhanNhay)
            {
                nhanNhay = false;
                vanTocY = lucNhay;

                RPC_Nhay();
            }
        }
        else
        {
            vanTocY += trongLuc * Runner.DeltaTime;
        }

        // ===== DI CHUYỂN =====
        Vector3 vanToc = huongDiChuyen * stats.Speed;
        vanToc.y = vanTocY;

        controller.Move(vanToc * Runner.DeltaTime);
        transform.position = controller.transform.position;

        // ===== ANIMATION =====
        if (anim != null)
        {
            anim.SetFloat("MoveX", inputMove.x, 0.1f, Runner.DeltaTime);
            anim.SetFloat("MoveY", inputMove.y, 0.1f, Runner.DeltaTime);

            bool dangChay = stats.IsRunning();
            anim.SetBool("IsRunning", dangChay && huongDiChuyen.magnitude > 0.1f);
        }

        // ===== XOAY =====
        if (huongDiChuyen.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(huongDiChuyen);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                Runner.DeltaTime * 10f
            );
        }
    }

    // ===== NAME =====
    public void SetPlayerName(string name)
    {
        if (tenPlayer != null)
            tenPlayer.SetName(name);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetName(string name)
    {
        TenMang = name;

        if (Object.HasStateAuthority)
        {
            var tatCaPlayer = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

            PlayerListManager.Instance.SetFullData("");

            foreach (var p in tatCaPlayer)
            {
                if (p.Object == null) continue;

                int id = p.Object.InputAuthority.PlayerId;
                string ten = p.GetPlayerName();

                if (string.IsNullOrEmpty(ten)) continue;

                PlayerStats stats = p.GetComponent<PlayerStats>();
                bool chet = stats != null && stats.HP <= 0;

                PlayerListManager.Instance.AddPlayer(id, ten);
                PlayerListManager.Instance.SetDead(id, chet);
            }

            RPC_SyncFullList(PlayerListManager.Instance.GetRawData());
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncFullList(string data)
    {
        if (PlayerListManager.Instance != null)
        {
            PlayerListManager.Instance.SetFullData(data);
        }
    }

    public void SetControl(bool value)
    {
        coTheDieuKhien = value;
    }

    public string GetPlayerName()
    {
        if (!string.IsNullOrEmpty(TenMang.ToString()))
            return TenMang.ToString();

        return "Player_" + Object.InputAuthority;
    }

    public void SetDead(bool dead)
{
    daChet = dead;
    coTheDieuKhien = !dead;

    if (dead && anim != null)
    {
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
        anim.SetBool("IsRunning", false);
    }
}
[Networked] private NetworkBool IsSitting { get; set; }
private Vector3 sitPosition;

// ===== SIT SYSTEM =====
public void SetSitting(bool value, Vector3 pos, Quaternion rot)
{
    if (!Object.HasStateAuthority)
    {
        RPC_SetSitting(value, pos, rot);
        return;
    }

    ApplySitting(value, pos, rot);
}

[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
void RPC_SetSitting(bool value, Vector3 pos, Quaternion rot)
{
    ApplySitting(value, pos, rot);
}

void ApplySitting(bool value, Vector3 pos, Quaternion rot)
{
    IsSitting = value;

    if (value)
    {
        controller.enabled = false;

        // 🔥 OFFSET CHUẨN (QUAN TRỌNG)
        Vector3 offset = new Vector3(0, -1f, -0.2f); 
        // chỉnh số này cho khớp model

        Vector3 finalPos = pos + rot * offset;

RPC_ApplySitTransform(finalPos, rot);

        if (anim != null)
            anim.SetBool("IsSitting", true);

        if (stats != null)
            stats.SetSitting(true);
    }
    else
    {
        controller.enabled = true;

        if (anim != null)
            anim.SetBool("IsSitting", false);

        if (stats != null)
            stats.SetSitting(false);
    }
}
public Vector2 GetMoveInput()
{
    if (input == null) return Vector2.zero;
    return input.actions["Move"].ReadValue<Vector2>();
}
[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_ApplySitTransform(Vector3 position, Quaternion rotation)
{
    controller.enabled = false;
    transform.position = position;
    transform.rotation = rotation;
}

}
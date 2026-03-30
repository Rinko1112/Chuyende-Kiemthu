using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local;

    private CharacterController characterController;
    private PlayerInput playerInput;
    private PlayerStats stats;
    private PlayerName playerName;

    private Animator animator;
    private Camera mainCamera;
    private bool isDead = false;

    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -5);
    [Networked] public NetworkString<_16> NetName { get; set; }

    [Header("HP BAR")]
    [SerializeField] private GameObject hpBarPrefab;
    private HPBarWorld hpBar;

    [Header("NAME TAG")]
    [SerializeField] private GameObject nameTagPrefab;
    private NameTag nameTag;

    private bool canControl = true;

    private float yVelocity;
    private float gravity = -20f;
    private float jumpForce = 8f;

    // ===== PUSH =====
    [Header("PUSH")]
    [SerializeField] private float pushDistance = 1f;
    [SerializeField] private LayerMask pushLayer;

    private bool isPushing = false;

    // 🔥 chống spam trigger
    private float lastPushTime = 0f;
    private float pushCooldown = 0.15f;

    [System.Obsolete]
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            mainCamera = Camera.main;

            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (!string.IsNullOrEmpty(savedName))
            {
                RPC_SetName(savedName);
            }
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        stats = GetComponent<PlayerStats>();
        playerName = GetComponent<PlayerName>();
        animator = GetComponent<Animator>();

        if (hpBarPrefab != null)
        {
            GameObject bar = Instantiate(hpBarPrefab);
            hpBar = bar.GetComponent<HPBarWorld>();
            hpBar.target = transform;
        }

        if (nameTagPrefab != null)
        {
            GameObject tag = Instantiate(nameTagPrefab, transform);
            nameTag = tag.GetComponent<NameTag>();
            nameTag.target = transform;
        }

        if (SpectatorCamera.Instance != null)
        {
            SpectatorCamera.Instance.RegisterPlayer(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (stats == null) return;

        if (Object.HasStateAuthority && canControl && !isDead)
        {
            Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
            Vector3 move = new Vector3(input.x, 0, input.y);

            if (move.magnitude > 1f)
                move = move.normalized;

            // ===== PUSH LOGIC =====
            if (move.magnitude > 0.1f)
            {
                Vector3 dir = move.normalized;

                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, pushDistance, pushLayer))
                {
                    Thung thung = hit.collider.GetComponent<Thung>();

                    if (thung != null && !thung.DangDiChuyen())
                    {
                        isPushing = true;

                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRot,
                            Runner.DeltaTime * 15f
                        );

                        thung.DiChuyen(dir);

                        // 🔥 SYNC TRIGGER QUA RPC
                        if (Time.time - lastPushTime > pushCooldown)
                        {
                            RPC_PlayPushAnim();
                            lastPushTime = Time.time;
                        }

                        return; // không move player
                    }
                }
            }

            isPushing = false;

            bool isHoldingShift = Keyboard.current.leftShiftKey.isPressed;

            if (isHoldingShift && move.magnitude > 0.1f && !stats.IsExhausted())
                stats.StartRunning();
            else
                stats.StopRunning();

            if (characterController.isGrounded)
            {
                if (yVelocity < 0)
                    yVelocity = -2f;

                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    yVelocity = jumpForce;

                    if (animator != null)
                        animator.SetTrigger("Jump"); // giữ nguyên
                }
            }
            else
            {
                yVelocity += gravity * Runner.DeltaTime;
            }

            Vector3 velocity = move * stats.Speed;
            velocity.y = yVelocity;

            characterController.Move(velocity * Runner.DeltaTime);
            transform.position = characterController.transform.position;

            if (animator != null)
            {
                animator.SetFloat("MoveX", input.x, 0.1f, Runner.DeltaTime);
                animator.SetFloat("MoveY", input.y, 0.1f, Runner.DeltaTime);

                bool isActuallyRunning = stats.IsRunning();
                animator.SetBool("IsRunning", isActuallyRunning && move.magnitude > 0.1f);
            }

            if (move.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Runner.DeltaTime * 10f
                );
            }
        }
    }

    // 🔥 RPC SYNC PUSH TRIGGER
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayPushAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger("Push");
        }
    }

    // ===== NAME =====

    public void SetPlayerName(string name)
    {
        if (playerName != null)
            playerName.SetName(name);

        if (nameTag != null)
            nameTag.SetName(name);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    [System.Obsolete]
    public void RPC_SetName(string name)
    {
        NetName = name;

        if (Object.HasStateAuthority)
        {
            var allPlayers = FindObjectsOfType<PlayerController>();

            PlayerListManager.Instance.SetFullData("");

            foreach (var p in allPlayers)
            {
                if (p.Object == null) continue;

                int id = p.Object.InputAuthority.PlayerId;
                string pname = p.GetPlayerName();

                if (string.IsNullOrEmpty(pname)) continue;

                PlayerStats stats = p.GetComponent<PlayerStats>();
                bool isDead = false;

                if (stats != null)
                {
                    isDead = stats.HP <= 0;
                }

                PlayerListManager.Instance.AddPlayer(id, pname);
                PlayerListManager.Instance.SetDead(id, isDead);
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
        canControl = value;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendChat(string playerName, string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.ReceiveMessage(playerName, message);
        }
    }

    public string GetPlayerName()
    {
        if (!string.IsNullOrEmpty(NetName.ToString()))
            return NetName.ToString();

        return "Player_" + Object.InputAuthority;
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        canControl = !dead;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class ThungBay : NetworkBehaviour
{
    [Header("UI PREFAB")]
    [SerializeField] private GameObject interactUIPrefab;

    private GameObject currentUI;
    private Button uiButton;
    private TMP_Text uiText;

    [Header("TEXT")]
    [SerializeField] private string textPick = "F - Nhặt";
    [SerializeField] private string textDrop = "F - Thả xuống";

    [Header("CONFIG")]
    [SerializeField] private float bayDoCao = 0.5f;
    [SerializeField] private float followDistance = 1.5f;

    [Header("SMOOTH")]
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float networkSmooth = 15f;

    [Header("PICKUP ANIM")]
    [SerializeField] private float pickupHeight = 1.2f;
    [SerializeField] private float pickupDuration = 0.25f;

    [Header("FLOAT EFFECT")]
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 3f;

    [Header("GROUND CHECK")]
    [SerializeField] private float groundOffset = 0.5f;
    [SerializeField] private float rayDistance = 5f;

    private PlayerController currentPlayer;
    private bool playerTrongVung = false;

    private float floatTime;
    private Vector3 velocity;

    private bool isPickingUp = false;
    private float pickupTimer = 0f;
    private Vector3 pickupStartPos;

    private bool isSpawned = false;

    [Networked] private NetworkBool DangDuocGiu { get; set; }
    [Networked] private PlayerRef Owner { get; set; }

    // 🔥 SYNC POSITION
    [Networked] private Vector3 NetworkPos { get; set; }

    public override void Spawned()
    {
        isSpawned = true;
        NetworkPos = transform.position;

        if (interactUIPrefab != null)
        {
            currentUI = Instantiate(interactUIPrefab);
            currentUI.SetActive(false);

            uiButton = currentUI.GetComponentInChildren<Button>();
            uiText = currentUI.GetComponentInChildren<TMP_Text>();

            if (uiButton != null)
            {
                uiButton.onClick.RemoveAllListeners();
                uiButton.onClick.AddListener(OnClickUI);
            }
        }
    }

    void Update()
    {
        if (!isSpawned) return;

        // ===== UI FOLLOW =====
        if (currentUI != null)
        {
            currentUI.transform.position = transform.position + Vector3.up * 2f;

            if (Camera.main != null)
                currentUI.transform.forward = Camera.main.transform.forward;
        }

        // ===== LOCAL INPUT =====
        if (PlayerController.Local != null)
        {
            bool isOwner = Owner == Runner.LocalPlayer;

            if (currentUI != null)
            {
                if (DangDuocGiu)
                    currentUI.SetActive(isOwner);
                else
                    currentUI.SetActive(playerTrongVung);
            }

            UpdateUIText(isOwner);

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (DangDuocGiu && !isOwner) return;

                if (DangDuocGiu || playerTrongVung)
                    ToggleCarry();
            }
        }

        // ===== CLIENT SMOOTH =====
        if (!Object.HasStateAuthority)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                NetworkPos,
                Time.deltaTime * networkSmooth
            );
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (DangDuocGiu && Owner != default)
        {
            PlayerController ownerPlayer = FindOwner();
            if (ownerPlayer == null) return;

            if (isPickingUp)
            {
                pickupTimer += Runner.DeltaTime;
                float t = pickupTimer / pickupDuration;

                float ease = Mathf.SmoothStep(0, 1, t);

                Vector3 target = pickupStartPos + Vector3.up * pickupHeight;
                transform.position = Vector3.Lerp(pickupStartPos, target, ease);

                if (t >= 1f)
                    isPickingUp = false;

                NetworkPos = transform.position;
                return;
            }

            Vector3 targetPos = ownerPlayer.transform.position
                - ownerPlayer.transform.forward * followDistance;

            targetPos.y += bayDoCao;

            floatTime += Runner.DeltaTime;
            targetPos.y += Mathf.Sin(floatTime * floatSpeed) * floatAmplitude;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref velocity,
                smoothTime
            );

            NetworkPos = transform.position;
        }
        else
        {
            NetworkPos = transform.position;
        }
    }

    PlayerController FindOwner()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.Object != null && p.Object.InputAuthority == Owner)
                return p;
        }

        return null;
    }

    void UpdateUIText(bool isOwner)
    {
        if (uiText == null) return;

        if (DangDuocGiu)
            uiText.text = isOwner ? textDrop : "";
        else
            uiText.text = textPick;
    }

    void ToggleCarry()
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestToggle();
            return;
        }

        SetCarry(!DangDuocGiu, Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestToggle(RpcInfo info = default)
    {
        PlayerRef sender = info.Source;

        if (DangDuocGiu && Owner != default && Owner != sender)
            return;

        SetCarry(!DangDuocGiu, sender);
    }

    void SetCarry(bool value, PlayerRef sender)
    {
        if (value)
        {
            if (Owner != default) return;

            Owner = sender; // 🔥 FIX CHUẨN
        }
        else
        {
            Owner = default;
        }

        DangDuocGiu = value;

        if (value)
        {
            isPickingUp = true;
            pickupTimer = 0f;
            pickupStartPos = transform.position;
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
            {
                Vector3 pos = hit.point;
                pos.y += groundOffset;
                transform.position = pos;
            }
        }

        NetworkPos = transform.position;

        RPC_SetState(DangDuocGiu, Owner);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetState(bool state, PlayerRef owner)
    {
        DangDuocGiu = state;
        Owner = owner;
    }

    void OnClickUI()
    {
        ToggleCarry();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.Object.HasInputAuthority)
        {
            playerTrongVung = true;
            currentPlayer = player;
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (currentPlayer == player)
        {
            playerTrongVung = false;
            currentPlayer = null;
        }
    }
}
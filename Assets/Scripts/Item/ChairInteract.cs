using UnityEngine;
using Fusion;
using TMPro;

public class ChairInteract : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactUIPrefab;

    [Header("SIT POINT (QUAN TRỌNG)")]
    [SerializeField] private Transform sitPoint;

    private GameObject currentUI;
    private TMP_Text uiText;

    private PlayerController currentPlayer;
    private bool playerInRange = false;

    private bool isSpawned = false;

    [Networked] private NetworkBool IsOccupied { get; set; }
    [Networked] private PlayerRef Occupier { get; set; }

    public override void Spawned()
    {
        isSpawned = true;

        if (interactUIPrefab != null)
        {
            currentUI = Instantiate(interactUIPrefab);
            currentUI.SetActive(false);

            uiText = currentUI.GetComponentInChildren<TMP_Text>();
        }
    }

    void Update()
    {
        if (!isSpawned) return;

        // ===== UI FOLLOW =====
        if (currentUI != null)
        {
            currentUI.transform.position = transform.position + Vector3.up * 1.5f;

            if (Camera.main != null)
                currentUI.transform.forward = Camera.main.transform.forward;
        }

        if (currentPlayer == null || !currentPlayer.Object.HasInputAuthority) return;

        bool isMeSitting = IsOccupied && Occupier == Runner.LocalPlayer;

        // 🔥 FIX: không hiện UI khi đã ngồi
        if (currentUI != null)
            currentUI.SetActive(playerInRange && !IsOccupied);

        if (uiText != null)
            uiText.text = "F - Ngồi";

        // ===== NGỒI =====
        if (playerInRange && !IsOccupied && Input.GetKeyDown(KeyCode.F))
        {
            Sit();
        }

        // ===== THOÁT NGỒI =====
        if (isMeSitting)
        {
            Vector2 move = currentPlayer.GetMoveInput();

            if (move.magnitude > 0.1f)
            {
                StandUp();
            }
        }
    }

    void Sit()
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestSit();
            return;
        }

        DoSit(Runner.LocalPlayer);
    }

    void StandUp()
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestStand();
            return;
        }

        DoStand();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestSit(RpcInfo info = default)
    {
        if (IsOccupied) return;

        DoSit(info.Source);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestStand(RpcInfo info = default)
    {
        if (Occupier != info.Source) return;

        DoStand();
    }

    void DoSit(PlayerRef player)
    {
        IsOccupied = true;
        Occupier = player;

        PlayerController p = FindPlayer(player);
        if (p != null && sitPoint != null)
        {
            p.SetSitting(true, sitPoint.position, sitPoint.rotation);
        }
    }

    void DoStand()
    {
        PlayerController p = FindPlayer(Occupier);
        if (p != null)
        {
            p.SetSitting(false, Vector3.zero, Quaternion.identity);
        }

        IsOccupied = false;
        Occupier = default;
    }

    PlayerController FindPlayer(PlayerRef player)
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.Object != null && p.Object.InputAuthority == player)
                return p;
        }

        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController p = other.GetComponentInParent<PlayerController>(); // 🔥 FIX

        if (p == null) return;

        if (p.Object.HasInputAuthority)
        {
            playerInRange = true;
            currentPlayer = p;
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController p = other.GetComponentInParent<PlayerController>(); // 🔥 FIX

        if (p == null) return;

        if (currentPlayer == p)
        {
            playerInRange = false;
            currentPlayer = null;

            if (currentUI != null)
                currentUI.SetActive(false); // 🔥 FIX HARD
        }
    }
}
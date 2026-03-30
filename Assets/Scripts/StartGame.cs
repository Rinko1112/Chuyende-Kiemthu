using UnityEngine;
using Fusion;

public class StartGame : NetworkBehaviour
{
    [Header("PLANE SPAWN")]
    [SerializeField] private GameObject spawnPlane;

    [Header("UI")]
    [SerializeField] private GameObject startButton;

    public override void Spawned()
    {
        // chỉ host thấy nút
        bool isHost = Object.HasStateAuthority;

        if (startButton != null)
            startButton.SetActive(isHost);
    }

    // ===== HOST NHẤN START =====
    public void OnClickStart()
    {
        if (!Object.HasStateAuthority) return;

        RPC_StartGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_StartGame()
    {
        // 🔥 Ẩn plane → player rơi xuống
        if (spawnPlane != null)
            spawnPlane.SetActive(false);

        // ẩn nút luôn
        if (startButton != null)
            startButton.SetActive(false);
    }
}
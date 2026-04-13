using UnityEngine;
using Fusion;

public class StartGame : NetworkBehaviour
{
    [Header("PLANE SPAWN")]
    [SerializeField] private Transform leftDoor;
[SerializeField] private Transform rightDoor;

    [Header("UI")]
    [SerializeField] private GameObject startButton;

    [Header("MOVE SETTINGS")]
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 2f;
    private Vector3 leftStartPos;
private Vector3 rightStartPos;

private Vector3 leftTargetPos;
private Vector3 rightTargetPos;

    private bool isMoving = false;

    public override void Spawned()
    {
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
        // 🔥 Ẩn nút
        if (startButton != null)
            startButton.SetActive(false);

        if (leftDoor != null && rightDoor != null)
{
    leftStartPos = leftDoor.position;
    rightStartPos = rightDoor.position;

    leftTargetPos = leftStartPos + Vector3.left * moveDistance;
    rightTargetPos = rightStartPos + Vector3.right * moveDistance;

    isMoving = true;
}

        // 🔥 START TIMER
        if (GameManager.Instance != null)
            GameManager.Instance.StartTimer();
    }

    private void Update()
{
    if (!isMoving) return; // 🔥 QUAN TRỌNG

    if (leftDoor != null)
    {
        leftDoor.position = Vector3.MoveTowards(
            leftDoor.position,
            leftTargetPos,
            moveSpeed * Time.deltaTime
        );
    }

    if (rightDoor != null)
    {
        rightDoor.position = Vector3.MoveTowards(
            rightDoor.position,
            rightTargetPos,
            moveSpeed * Time.deltaTime
        );
    }

    if (Vector3.Distance(leftDoor.position, leftTargetPos) < 0.01f &&
        Vector3.Distance(rightDoor.position, rightTargetPos) < 0.01f)
    {
        isMoving = false;
    }
}
}
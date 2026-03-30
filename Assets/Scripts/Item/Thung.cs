using UnityEngine;
using Fusion;

public class Thung : NetworkBehaviour
{
    [Header("MOVE")]
    public float moveSpeed = 5f;

    [Header("DELAY")]
    public float pushDelay = 0.25f;

    [Networked] private Vector3 TargetPos { get; set; }
    [Networked] private TickTimer delayTimer { get; set; }

    private bool isMoving = false;

    public bool DangDiChuyen()
    {
        return isMoving;
    }

    Vector3 SnapDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        else
            return new Vector3(0, 0, Mathf.Sign(dir.z));
    }

    public override void Spawned()
    {
        SnapNow();
        TargetPos = transform.position;
        delayTimer = TickTimer.None;
    }

    void SnapNow()
    {
        if (GridDebug.Instance == null) return;

        transform.position = GridDebug.Instance.SnapToGrid(transform.position);
    }

    public void DiChuyen(Vector3 huong)
    {
        if (isMoving) return;

        huong = SnapDirection(huong);
        RPC_RequestMove(huong);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestMove(Vector3 huong)
    {
        if (GridDebug.Instance == null) return;

        if (!delayTimer.ExpiredOrNotRunning(Runner))
            return;

        if (isMoving)
            return;

        huong = SnapDirection(huong);

        Vector3 current = TargetPos;
        Vector3 target = current + huong * GridDebug.Instance.cellSize;

        Vector3 boxCenter = target;
        Vector3 halfExtents = new Vector3(0.4f, 0.4f, 0.4f);

        Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents);

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            if (!hit.isTrigger)
            {
                return;
            }
        }

        delayTimer = TickTimer.CreateFromSeconds(Runner, pushDelay);
        RPC_StartMove(target);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_StartMove(Vector3 target)
    {
        TargetPos = target;

        StopAllCoroutines();
        StartCoroutine(MoveTo(target));
    }

    System.Collections.IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Runner.DeltaTime
            );

            yield return null;
        }

        if (GridDebug.Instance != null)
            transform.position = GridDebug.Instance.SnapToGrid(target);

        isMoving = false;
    }
}
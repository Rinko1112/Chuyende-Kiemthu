using UnityEngine;
using Fusion;

public class GiantSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject giantPrefab;

    [Header("SPAWN")]
    [SerializeField] private Transform spawnPoint;

    [Header("PATROL")]
    [SerializeField] private float patrolRadius = 10f;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        SpawnGiant();
    }

    void SpawnGiant()
    {
        if (giantPrefab == null || spawnPoint == null) return;

        NetworkObject giantObj = Runner.Spawn(
            giantPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        GiantGolem giant = giantObj.GetComponent<GiantGolem>();

        // 🔥 tạo patrol center riêng
        GameObject patrolCenter = new GameObject("PatrolCenter");
        patrolCenter.transform.position = spawnPoint.position;

        if (giant != null)
        {
            giant.SetPatrol(patrolCenter.transform, patrolRadius);
        }
    }
}
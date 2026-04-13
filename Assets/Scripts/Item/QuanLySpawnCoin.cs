using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class QuanLySpawnCoin : NetworkBehaviour
{
    [Header("COIN PREFAB")]
    [SerializeField] private NetworkObject coinPrefab;

    [Header("GROUND")]
    [SerializeField] private Transform ground;

    [Header("SỐ LƯỢNG COIN")]
    [SerializeField] private int soLuongCoin = 50;

    [Header("GRID")]
    [SerializeField] private float khoangCach = 2f;

    [Header("OBSTACLE")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("OFFSET")]
    [SerializeField] private float heightOffset = 0.2f;

    private List<Vector3> danhSachViTriHopLe = new List<Vector3>();

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        LayDanhSachViTri();
        TronDanhSach();
        SpawnCoin();
    }

    void LayDanhSachViTri()
    {
        danhSachViTriHopLe.Clear();

        var renderer = ground.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        for (float x = bounds.min.x; x < bounds.max.x; x += khoangCach)
        {
            for (float z = bounds.min.z; z < bounds.max.z; z += khoangCach)
            {
                Vector3 pos = new Vector3(x, bounds.max.y + 5f, z);

                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 50f))
                {
                    if (hit.transform != ground) continue;

                    Vector3 finalPos = hit.point;

                    if (Physics.CheckSphere(finalPos, 0.4f, obstacleLayer))
                        continue;

                    var col = coinPrefab.GetComponentInChildren<Collider>();
                    if (col != null)
                        finalPos.y += col.bounds.extents.y;

                    finalPos.y += heightOffset;

                    danhSachViTriHopLe.Add(finalPos);
                }
            }
        }
    }

    void TronDanhSach()
    {
        for (int i = 0; i < danhSachViTriHopLe.Count; i++)
        {
            int randomIndex = Random.Range(i, danhSachViTriHopLe.Count);

            Vector3 temp = danhSachViTriHopLe[i];
            danhSachViTriHopLe[i] = danhSachViTriHopLe[randomIndex];
            danhSachViTriHopLe[randomIndex] = temp;
        }
    }

    void SpawnCoin()
    {
        int count = Mathf.Min(soLuongCoin, danhSachViTriHopLe.Count);

        for (int i = 0; i < count; i++)
        {
            Runner.Spawn(
                coinPrefab,
                danhSachViTriHopLe[i],
                Quaternion.identity
            );
        }
    }
}
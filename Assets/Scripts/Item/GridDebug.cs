using UnityEngine;
using System.Collections.Generic;

public class GridDebug : MonoBehaviour
{
    public static GridDebug Instance;

    [Header("GRID SIZE")]
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;

    [Header("HIỂN THỊ")]
    public bool hienTrongGame = true;
    public Material lineMaterial;

    [Header("TƯỜNG")]
    public GameObject wallPrefab;
    public bool taoTuong = true;

    // ===== 🔥 THÊM PHẦN NÀY =====
    [System.Serializable]
    public class ThungSpawnData
    {
        public GameObject prefab;
        public int x;
        public int z;
    }

    [Header("SPAWN THÙNG")]
    public List<ThungSpawnData> danhSachThung = new List<ThungSpawnData>();
    // ============================

    private void Awake()
    {
        Instance = this;
    }

    // ===== SNAP =====
    public Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Floor((pos.x - transform.position.x) / cellSize) * cellSize;
        float z = Mathf.Floor((pos.z - transform.position.z) / cellSize) * cellSize;

        x += cellSize * 0.5f;
        z += cellSize * 0.5f;

        return new Vector3(
            x + transform.position.x,
            pos.y,
            z + transform.position.z
        );
    }

    // 🔥 SNAP THEO Ô CHUẨN TUYỆT ĐỐI (dùng cho spawn)
    public Vector3 GetCellCenter(int x, int z)
    {
        return transform.position + new Vector3(
            x * cellSize + cellSize * 0.5f,
            0,
            z * cellSize + cellSize * 0.5f
        );
    }

    // ===== VẼ GRID TRONG GAME =====
    private void Start()
    {
        if (hienTrongGame)
        {
            VeGridRuntime();
        }

        if (taoTuong)
        {
            TaoTuongBao();
        }

        // 🔥 SPAWN THÙNG
        SpawnThung();
    }

    void SpawnThung()
    {
        foreach (var data in danhSachThung)
        {
            if (data.prefab == null) continue;

            Vector3 pos = GetCellCenter(data.x, data.z);
            pos.y = 0.5f; // giữ đúng chiều cao thùng

            Instantiate(data.prefab, pos, Quaternion.identity, transform);
        }
    }

    void VeGridRuntime()
    {
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = transform.position + new Vector3(x * cellSize, 0.01f, 0);
            Vector3 end = transform.position + new Vector3(x * cellSize, 0.01f, height * cellSize);

            TaoLine(start, end);
        }

        for (int z = 0; z <= height; z++)
        {
            Vector3 start = transform.position + new Vector3(0, 0.01f, z * cellSize);
            Vector3 end = transform.position + new Vector3(width * cellSize, 0.01f, z * cellSize);

            TaoLine(start, end);
        }
    }

    void TaoLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;

        lr.material = lineMaterial;
        lr.useWorldSpace = true;
    }

    // ===== TẠO TƯỜNG BAO =====
    void TaoTuongBao()
    {
        if (wallPrefab == null) return;

        int cuaVao = width / 2;

        for (int x = 0; x < width; x++)
        {
            if (x == cuaVao) continue;

            SpawnWall(x, -1);
            SpawnWall(x, height);
        }

        for (int z = 0; z < height; z++)
        {
            SpawnWall(-1, z);
            SpawnWall(width, z);
        }
    }

    void SpawnWall(int x, int z)
    {
        Vector3 pos = GetCellCenter(x, z);
        pos.y = 0.5f;

        Instantiate(wallPrefab, pos, Quaternion.identity, transform);
    }

    // ===== GIZMOS =====
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 origin = transform.position;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(x * cellSize, 0, height * cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= height; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * cellSize);
            Vector3 end = origin + new Vector3(width * cellSize, 0, z * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}
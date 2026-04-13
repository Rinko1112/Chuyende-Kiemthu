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

    [Header("GROUND LAYER")]
    [SerializeField] private LayerMask groundLayer;

    [Header("OFFSET CHIỀU CAO")]
    [SerializeField] private float spawnHeightOffset = 0.3f;

    [System.Serializable]
    public class ThungSpawnData
    {
        public GameObject prefab;
        public int x;
        public int z;
    }

    [Header("SPAWN THÙNG")]
    public List<ThungSpawnData> danhSachThung = new List<ThungSpawnData>();

    // 🔥 CONTAINER
    private Transform thungContainer;
    private Transform wallContainer;
    private Transform gridLineContainer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 🔥 TẠO CONTAINER
        thungContainer = TaoContainer("ThungContainer");
        wallContainer = TaoContainer("WallContainer");
        gridLineContainer = TaoContainer("GridLineContainer");

        if (hienTrongGame)
            VeGridRuntime();

        if (taoTuong)
            TaoTuongBao();

        SpawnThung();
    }

    Transform TaoContainer(string ten)
    {
        GameObject obj = new GameObject(ten);
        obj.transform.SetParent(transform);
        return obj.transform;
    }

    // ================= GRID =================
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

    public Vector3 GetCellCenter(int x, int z)
    {
        return transform.position + new Vector3(
            x * cellSize + cellSize * 0.5f,
            0,
            z * cellSize + cellSize * 0.5f
        );
    }

    // ================= THÙNG =================
    void SpawnThung()
    {
        foreach (var data in danhSachThung)
        {
            if (data.prefab == null) continue;

            Vector3 pos = GetCellCenter(data.x, data.z);

            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out hit, 50f, groundLayer))
                pos.y = hit.point.y;
            else
                pos.y = transform.position.y;

            var col = data.prefab.GetComponentInChildren<Collider>();
            if (col != null)
                pos.y += col.bounds.extents.y;

            pos.y += spawnHeightOffset;

            GameObject obj = Instantiate(data.prefab, pos, Quaternion.identity, thungContainer);

            obj.transform.position = SnapToGrid(obj.transform.position);
        }
    }

    // ================= GRID LINE =================
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
        lineObj.transform.SetParent(gridLineContainer); // 🔥 GOM VÀO CONTAINER

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;

        lr.material = lineMaterial;
        lr.useWorldSpace = true;
    }

    // ================= TƯỜNG =================
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
        Vector3 pos = transform.position;

        if (x == -1)
        {
            pos.x += 0;
            pos.z += z * cellSize + cellSize * 0.5f;
        }
        else if (x == width)
        {
            pos.x += width * cellSize;
            pos.z += z * cellSize + cellSize * 0.5f;
        }
        else if (z == -1)
        {
            pos.x += x * cellSize + cellSize * 0.5f;
            pos.z += 0;
        }
        else if (z == height)
        {
            pos.x += x * cellSize + cellSize * 0.5f;
            pos.z += height * cellSize;
        }

        var col = wallPrefab.GetComponentInChildren<Collider>();
        float yOffset = col != null ? col.bounds.extents.y : 0.5f;
        pos.y = transform.position.y + yOffset;

        Quaternion rot = Quaternion.identity;

        if (x == -1 || x == width)
            rot = Quaternion.Euler(0, 90f, 0);

        Instantiate(wallPrefab, pos, rot, wallContainer);
    }

    // ================= GIZMOS =================
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
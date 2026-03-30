using UnityEngine;
using Fusion;

public class Coin : NetworkBehaviour
{
    [SerializeField] private int value = 10;

    [Header("ROTATE")]
    [SerializeField] private float rotateSpeed = 180f;

    private bool collected = false;

    void Update()
    {
        // 🔥 TẤT CẢ CLIENT đều chạy → ai cũng thấy xoay
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (collected) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        collected = true;

        player.RPC_AddScore(value);

        Runner.Despawn(Object);
    }
}
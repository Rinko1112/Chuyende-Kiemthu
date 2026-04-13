using UnityEngine;
using Fusion;

public class Coin : NetworkBehaviour
{
    [SerializeField] private int value = 10;
    [SerializeField] private string itemId = "coin";

    [Header("ROTATE")]
    [SerializeField] private float rotateSpeed = 180f;

    private bool collected = false;

    public override void FixedUpdateNetwork()
    {
        // 🔥 CHỈ SERVER xoay
        if (!Object.HasStateAuthority) return;

        transform.Rotate(Vector3.up * rotateSpeed * Runner.DeltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (collected) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        collected = true;

        player.RPC_AddItem(itemId);
        player.RPC_AddScore(value);

        Runner.Despawn(Object);
    }
}
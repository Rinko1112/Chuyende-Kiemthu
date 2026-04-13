using UnityEngine;
using Fusion;

public class KeyPickup : NetworkBehaviour
{
    [SerializeField] private string itemId = "key";
    [SerializeField] private Sprite icon;

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        // 🔥 add vào inventory network
        player.RPC_AddItem(itemId);

        Runner.Despawn(Object);
    }
}
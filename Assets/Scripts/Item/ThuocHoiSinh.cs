using UnityEngine;
using Fusion;
using System.Linq;

public class ThuocHoiSinh : NetworkBehaviour
{
    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (used) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        used = true;

        ReviveOnePlayer(player.transform);

        Runner.Despawn(Object);
    }

    void ReviveOnePlayer(Transform healer)
    {
        var allPlayers = FindObjectsOfType<PlayerStats>();

        // 🔥 lấy player chết đầu tiên
        PlayerStats dead = allPlayers.FirstOrDefault(p => p.HP <= 0);

        if (dead == null) return;

        Vector3 spawnPos = healer.position + healer.forward * 1.5f;

        dead.Revive(spawnPos);

        // 🔥 add lại spectator
        if (SpectatorCamera.Instance != null)
        {
            SpectatorCamera.Instance.AddPlayer(dead.transform);
        }
    }
}
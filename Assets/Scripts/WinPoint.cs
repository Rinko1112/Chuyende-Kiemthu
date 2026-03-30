using UnityEngine;
using Fusion;

public class WinPoint : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin();
        }
    }
}
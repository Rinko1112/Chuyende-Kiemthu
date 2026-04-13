using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject[] playerPrefabs;

    [Header("SPAWN POINT")]
    [SerializeField] private Transform spawnPoint;

    [Header("RANDOM OFFSET")]
    [SerializeField] private float spawnRadius = 2f;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Vector3 basePos = Vector3.zero;

            if (spawnPoint != null)
                basePos = spawnPoint.position;

            Vector3 offset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Vector3 spawnPos = basePos + offset;

            // 🔥 random nhân vật
            int index = Random.Range(0, playerPrefabs.Length);
            GameObject chosenPrefab = playerPrefabs[index];

            Runner.Spawn(chosenPrefab, spawnPos, Quaternion.identity, player);
        }
    }
}
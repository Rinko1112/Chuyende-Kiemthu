using UnityEngine;
using System.Collections.Generic;

public class SpectatorCamera : MonoBehaviour
{
    public static SpectatorCamera Instance;

    private List<Transform> players = new List<Transform>();
    private int currentIndex = 0;

    [SerializeField] private Vector3 offset = new Vector3(0, 10, -5);

    [Header("SMOOTH SETTINGS")]
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 velocity;

    [Header("FIXED ROTATION")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(60f, 0f, 0f); // 🔥 góc cố định

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(Transform t)
    {
        if (!players.Contains(t))
            players.Add(t);
    }

    public void RemovePlayer(Transform t)
    {
        if (players.Contains(t))
            players.Remove(t);

        if (currentIndex >= players.Count)
            currentIndex = 0;
    }

    public void ActivateSpectator()
    {
        players.RemoveAll(p => p == null);

        if (players.Count == 0) return;

        currentIndex = 0;
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (players.Count == 0) return;

        // ===== SWITCH PLAYER =====
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % players.Count;
            velocity = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = players.Count - 1;

            velocity = Vector3.zero;
        }

        Transform target = players[currentIndex];
        if (target == null) return;

        // ===== POSITION =====
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // ===== FIXED ROTATION (KHÔNG RUNG) =====
        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    public void AddPlayer(Transform t)
    {
        if (!players.Contains(t))
            players.Add(t);
    }
}
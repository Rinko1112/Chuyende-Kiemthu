using UnityEngine;
using System.Collections.Generic;

public class SpectatorCamera : MonoBehaviour
{
    public static SpectatorCamera Instance;

    private List<Transform> players = new List<Transform>();
    private int currentIndex = 0;

    [SerializeField] private Vector3 offset = new Vector3(0, 10, -5);

    [Header("SMOOTH SETTINGS")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector3 velocity; // dùng cho SmoothDamp

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
    // 🔥 remove player chết khỏi list
    players.RemoveAll(p => p == null);

    if (players.Count == 0) return;

    currentIndex = 0;
    velocity = Vector3.zero;
}

    void LateUpdate() // 🔥 dùng LateUpdate để mượt hơn camera follow
    {
        if (players.Count == 0) return;

        // đổi player
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % players.Count;
            velocity = Vector3.zero; // reset khi đổi target
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = players.Count - 1;

            velocity = Vector3.zero;
        }

        Transform target = players[currentIndex];

        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // 🔥 mượt position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // 🔥 mượt rotation
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    public void AddPlayer(Transform t)
{
    if (!players.Contains(t))
        players.Add(t);
}
    
}
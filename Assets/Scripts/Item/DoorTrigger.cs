using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Door door;

    void OnTriggerEnter(Collider other)
    {
        if (door != null)
            door.OnPlayerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (door != null)
            door.OnPlayerExit(other);
    }
}
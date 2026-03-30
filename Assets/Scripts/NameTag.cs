using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Transform target;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + Vector3.up * 2.5f;
        transform.forward = cam.transform.forward;
    }

    public void SetName(string name)
    {
        text.text = name;
    }
}
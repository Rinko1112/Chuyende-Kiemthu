using Fusion;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    [Networked] public string Nickname { get; set; }

    private NameTag nameTag;

    public override void Spawned()
    {
        nameTag = GetComponentInChildren<NameTag>();
    }

    void Update()
    {
        if (nameTag == null)
            nameTag = GetComponentInChildren<NameTag>();

        // 🔥 luôn update nếu có nickname
        if (!string.IsNullOrEmpty(Nickname) && nameTag != null)
        {
            nameTag.SetName(Nickname);
        }
    }

    public void SetName(string name)
    {
        Nickname = name;
    }
}
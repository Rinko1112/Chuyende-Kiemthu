using Fusion;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    [Networked] public string Nickname { get; set; }

    public void SetName(string name)
    {
        Nickname = name;
    }
}
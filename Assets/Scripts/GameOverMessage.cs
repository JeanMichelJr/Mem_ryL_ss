using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMessage : MonoBehaviour
{
    public Text message;

    private void Awake()
    {
        if (Player.instance != null && Player.instance.deathReason != null && Player.instance.deathReason != "")
        {
            message.text = Player.instance.deathReason;
        }
    }
}
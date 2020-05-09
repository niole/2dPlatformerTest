using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveEvents : MonoBehaviour
{
    public static PlayerMoveEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<float> onPlayerMoveTriggerEnter;

    public void PlayerMoveTriggerEnter(float xLocation)
    {

        if (onPlayerMoveTriggerEnter != null)
        {
            onPlayerMoveTriggerEnter(xLocation);
        }
    }
}

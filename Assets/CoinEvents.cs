using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinEvents : MonoBehaviour
{

    public static CoinEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<float> onCoinDestroyedTriggerEnter;

    public void CoinDestroyedTriggerEnter(float xLocation)
    {

        if (onCoinDestroyedTriggerEnter != null)
        {
            onCoinDestroyedTriggerEnter(xLocation);
        }
    }
}

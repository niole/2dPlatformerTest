using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : MonoBehaviour
{

    bool isDestroyed = false;

    int totalFlickers = 0;

    int maxFlixers = 30;

    Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (isDestroyed)
        {
            totalFlickers += 1;
            if (totalFlickers < maxFlixers && totalFlickers%2 == 0)
            {
                renderer.enabled = !renderer.enabled;
            }

            if (totalFlickers == maxFlixers)
            {
                Destroy(gameObject);
            }

        }
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            isDestroyed = true;
        }
    }
}

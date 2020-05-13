using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : MonoBehaviour
{

    bool isDestroyed = false;

    int totalFlickers = 0;

    int maxFlixers = 30;

    Renderer renderer;

    float renderedXPosition;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        renderedXPosition = transform.position.x;
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
                CoinEvents.current.CoinDestroyedTriggerEnter(renderedXPosition);
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

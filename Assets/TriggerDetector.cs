using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Instantiate a trigger detector in the right places
 */
public class TriggerDetector : MonoBehaviour
{
    public int x;

    public int y;

    public bool isLeftTrigger;

    public bool isRightTrigger;

    public static TriggerDetector current;

    private float playerX = 0f;

    private string direction = "stopped";

    private void Awake()
    {
        current = this;
    }

    public event Action<bool, int, int> onTrigger;

    void Start()
    {
        PlayerMoveEvents.current.onPlayerMoveTriggerEnter += OnPlayerMove;
    }

    private void OnPlayerMove(float xLocation)
    {
        if (xLocation < playerX)
        {
            direction = "left";
        } else if (xLocation > playerX)
        {
            direction = "right";
        } else
        {
            direction = "stopped";
        }
        playerX = xLocation;
    }

    /**
     * going left, going right, did it trigger?
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (onTrigger != null)
            {
                if (isLeftTrigger && GoingLeft())
                {
                    onTrigger(true, x, y);
                } else if (isRightTrigger && GoingRight())
                {
                    onTrigger(false, x, y);
                }
            }
        }
    }

    bool GoingRight()
    {
        return direction == "right";
    }

    bool GoingLeft()
    {
        return direction == "left";
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Game manager maanges
 * score
 */

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Text scoreElement;

    private float totalPoints = 0f;

    void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        scoreElement.text = $"{totalPoints}";
    }

    public float AddPoints(float points)
    {
        totalPoints += points;
        return totalPoints;
    }

    public float SubtractPoints(float points)
    {
        totalPoints -= points;
        return totalPoints;
    }

    public float GetPoints()
    {
        return totalPoints;
    }
}

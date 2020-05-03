using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        Vector3 pPos = player.position;
        transform.position = new Vector3(pPos.x, pPos.y, transform.position.z);
    }
}

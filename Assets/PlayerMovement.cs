﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class PlayerMovement : MonoBehaviour
{
    Animator animator;

    Rigidbody2D rb;

    SpriteRenderer spriteRenderer;

    float speed = 50f;

    float jumpSpeed = 325f;

    bool jumping = false;

    float terminalSpeed = 3f;

    float farHitDistance = 1f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!jumping)
        {
            LayerMask mask = LayerMask.GetMask("Ground");
            RaycastHit2D farRightHit = Physics2D.Raycast(transform.position, Vector2.right, farHitDistance, mask);
            RaycastHit2D farLeftHit = Physics2D.Raycast(transform.position, -Vector2.right, farHitDistance, mask);
            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, -Vector2.up, Mathf.Infinity, mask);

            float distanceDown = hitDown.distance;

            if (farRightHit.collider != null)
            {
                transform.up = GetWeightedNormal(farRightHit, hitDown);
            } else if (farLeftHit.collider != null)
            {
                transform.up = GetWeightedNormal(farLeftHit, hitDown);
            } else if (hitDown.collider != null)
            {
                transform.up = hitDown.normal;
            }
        }

        float x = Input.GetAxis("Horizontal");
        if (x != 0)
        {
            animator.Play("RunningPlayer");
        } else
        {
            animator.Play("IdlePlayer");
        }

        if (x > 0)
        {
            spriteRenderer.flipX = false;
        } else if (x < 0)
        {
            spriteRenderer.flipX = true;
        }

        Vector2 xVelocity = Vector2.ClampMagnitude(Vector2.right * (rb.velocity.x  + x * Time.fixedDeltaTime * speed), terminalSpeed);
        Vector2 yVelocity = Vector2.up * rb.velocity.y + GetYVelocity();
        rb.velocity = xVelocity + yVelocity;

        float y = Input.GetAxis("Jump");
        if (y > 0)
        {
            jumping = true;
        }
    }

    Vector2 GetWeightedNormal(RaycastHit2D hit1, RaycastHit2D hit2)
    {
        float hit1Distance = hit1.distance;
        float hit2Distance = hit2.distance;

        float totalDistance = hit1Distance + hit2Distance;
        float proportion1 = hit1Distance/totalDistance;
        float proportion2 = hit2Distance/totalDistance;
        return hit1.normal * proportion2 + hit2.normal * proportion1;
    }

    Vector2 GetYVelocity()
    {
        float y = Input.GetAxis("Jump");
        if (jumping)
        {
            return Vector2.zero;
        } else
        {
            return Vector2.up * y * Time.fixedDeltaTime * jumpSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        string otherTag = other.gameObject.tag;
        switch (otherTag)
        {
            case "Ground":
                jumping = false;
                break;
            case "Coin":
                Debug.Log("coin");
                GameManager.Instance.AddPoints(1f);
                break;
            case "Enemy":
                GameManager.Instance.SubtractPoints(1f);
                break;
            default:
                break;
        }
    }
}

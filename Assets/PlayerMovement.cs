using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Animator animator;

    Rigidbody2D rb;

    SpriteRenderer spriteRenderer;

    float speed = 50f;

    float jumpSpeed = 200f;

    bool jumping = false;

    float terminalSpeed = 3f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 1f, mask);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, -Vector2.right, 1f, mask);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, -Vector2.up, 1f, mask);

        if (hitRight.collider != null)
        {
            transform.up = hitRight.normal;
        } else if (hitDown.collider != null)
        {
            transform.up = hitDown.normal;
        } else if (hitLeft != null)
        {
            transform.up = hitLeft.normal;
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
        if (other.gameObject.tag == "Ground")
        {
            jumping = false;
        }
    }
}

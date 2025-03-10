using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour {
    [SerializeField] private float speed = 8f;
    [SerializeField] private float JumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody2D rb;
    private float horizontal;
    private bool isFacingRight = true;
    public Animator animator;

    //Dash fields
    [SerializeField] private TrailRenderer tr;
    private bool isDashing;
    private bool canDash = true;
    private float dashingPower = 44f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    //Coyote time
    private  float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    //Jump buffer
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    // //Wallslide
    // private bool isWallSliding;
    // private float wallSlidingSpeed = 2f;
    // [SerializeField] private Transform wallCheck;
    // [SerializeField] private LayerMask wallLayer;

    void Update() {
        if(jumpBufferCounter > 0) {
            jumpBufferCounter -= Time.deltaTime;
        }

        if(IsGrounded()) {
            coyoteTimeCounter = coyoteTime;
        }
        else  {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // WallSlide();

        // if(isWallSliding) {
        //     return;
        // }

        if(isDashing) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        Flip();
    }

    void FixedUpdate() {

        if(isDashing) {
            return;
        }
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if(rb.velocity.y < 0) {
         rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -JumpForce));
        }
    }

    public void Move(InputAction.CallbackContext context) {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context) {
        if(context.performed) {
            jumpBufferCounter = jumpBufferTime;
        }
   
        if(jumpBufferCounter > 0f && coyoteTimeCounter > 0f) {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            jumpBufferCounter = 0f;
        }

        if(context.canceled && rb.velocity.y > 0f) {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            coyoteTimeCounter = 0f;
        }
    }

    private bool IsGrounded() {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip() {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f) {
            isFacingRight = !isFacingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // private bool IsWalled() {
    //     return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    // }

    // private void WallSlide() {
    //     if(IsWalled() && !IsGrounded() && horizontal != 0f) {
    //          isWallSliding = true;
    //         rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.x, -wallSlidingSpeed, float.MaxValue));

    //     }
    //     else {
    //         isWallSliding = false;
    //     }
    // }

    private IEnumerator Dash() {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * dashingPower, 0f);
        rb.gravityScale = 0;
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
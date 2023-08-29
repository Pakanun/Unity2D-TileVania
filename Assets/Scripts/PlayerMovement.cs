using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float climbSpeed;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] AudioClip fireSFX;

    Vector2 moveInput;
    Rigidbody2D rb;
    Animator anim;
    CapsuleCollider2D bodyCollider;
    BoxCollider2D feetCollider;
    float gravityAtStart;

    bool isAlive = true;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        feetCollider = GetComponent<BoxCollider2D>();
        gravityAtStart = rb.gravityScale;
        
    }

    
    void Update()
    {
        if (!isAlive) { return; }
        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);
    }

    void OnJump(InputValue value) 
    {
        if (!isAlive) { return; }
        if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }
        if(value.isPressed) 
        {
            rb.velocity += new Vector2(0f, jumpSpeed);
            AudioSource.PlayClipAtPoint(jumpSFX, Camera.main.transform.position);
        }
    }

    void OnFire(InputValue value)
    {
        if (!isAlive) { return; }
        Instantiate(bullet, gun.position, transform.rotation);
        AudioSource.PlayClipAtPoint(fireSFX, Camera.main.transform.position);
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * playerSpeed, rb.velocity.y);
        rb.velocity = playerVelocity;

        bool playerMove = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

        anim.SetBool("isRunning", playerMove);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

        if(playerHasHorizontalSpeed) 
        { 
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }

    }

    void ClimbLadder()
    {
        if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        {
            rb.gravityScale = gravityAtStart;
            anim.SetBool("isClimbing", false);
            return; 
        }

        Vector2 climbVelocity = new Vector2(rb.velocity.x, moveInput.y * climbSpeed);
        rb.velocity = climbVelocity;
        rb.gravityScale = 0f;

        bool playerMove = Mathf.Abs(rb.velocity.y) > Mathf.Epsilon;
        anim.SetBool("isClimbing", playerMove);

    }

    void Die()
    {
        if(bodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazard")))
        {
            isAlive = false;
            anim.SetTrigger("Dying");
            rb.velocity = deathKick;
            FindAnyObjectByType<GameSession>().ProcessPlayerDeath();
        }
    }
}

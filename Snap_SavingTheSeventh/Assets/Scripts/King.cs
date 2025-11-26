using UnityEngine;

public class King : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Respawn")]
    public Vector2 respawnPoint;
    public float fallLimitY = -10f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();      // <—— tambahkan animator
        respawnPoint = transform.position; 
    }

    void Update()
    {
        Move();
        Jump();
        CheckFall();
        UpdateAnimations();                   // <—— update animasi setiap frame
    }

    void Move()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip kiri/kanan
        if (move > 0 && !isFacingRight)
            Flip();
        else if (move < 0 && isFacingRight)
            Flip();
    }

    void Jump()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void CheckFall()
    {
        if (transform.position.y < fallLimitY)
        {
            transform.position = respawnPoint;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }


    //  ANIMATION
    void UpdateAnimations()
    {
        // Parameter "Speed" = nilai absolute kecepatan horizontal
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    // Visual debug untuk ground check
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

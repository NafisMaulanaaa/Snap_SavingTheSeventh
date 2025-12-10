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

    [Header("Map Boundary")]
    public float minX;
    public float maxX;

    [Header("Vertical Boundary")]
    public float maxY;

    private Rigidbody2D rb;
    public Animator anim;
    private bool isFacingRight = true;
    private bool isGrounded;

    private bool hasGroundedParam = false;
    private float moveInput;

    [Header("Attack")]
    public float attackDuration = 0.4f; 
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        respawnPoint = transform.position;

        if (rb == null)
        {
            Debug.LogError("[King] Rigidbody2D tidak ditemukan.");
            enabled = false;
            return;
        }

        if (groundCheck == null)
        {
            Debug.LogError("[King] groundCheck belum di-assign.");
            enabled = false;
            return;
        }

        if (anim != null)
        {
            foreach (var p in anim.parameters)
            {
                if (p.name == "Grounded" && p.type == AnimatorControllerParameterType.Bool)
                {
                    hasGroundedParam = true;
                    break;
                }
            }
        }

        if (anim == null)
        {
            Debug.LogError("GAWAT: Animator masih kosong!");
        }
        else
        {
            Debug.Log("AMAN: Animator terhubung ke: " + anim.gameObject.name);

            bool paramFound = false;
            foreach (var param in anim.parameters)
            {
                if (param.name == "1_Move") paramFound = true;
            }

            if (paramFound) Debug.Log("AMAN: Parameter 1_Move ditemukan.");
            else Debug.LogError("GAWAT: Parameter 1_Move tidak ditemukan!");
        }
    }

    void Update()
    {
        Move();
        CheckGround();
        Jump();
        CheckFall();
        ClampPosition();
        Attack();
        UpdateAnimations();
    }

    void Move()
    {
        if (isAttacking) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        if (!isGrounded || isAttacking) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    void Attack()
    {
        if (isAttacking) return;

        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;

            // Stop movement
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("2_Attack", true);
            }

            StartCoroutine(ResetAttack());
        }
    }

    System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackDuration);

        if (anim != null)
        {
            anim.SetBool("2_Attack", false);
        }

        isAttacking = false;
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

    void ClampPosition()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (pos.y > maxY)
            pos.y = maxY;

        transform.position = pos;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isWalking = moveInput != 0 && !isAttacking;
        anim.SetBool("1_Move", isWalking);

        bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded && !isAttacking;
        anim.SetBool("7_Jump", isJumping);

        if (hasGroundedParam)
            anim.SetBool("Grounded", isGrounded);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
